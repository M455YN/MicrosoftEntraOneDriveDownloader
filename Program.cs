using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MicrosoftEntraOneDriveDownloader
{
    class Program
    {
        #region OAuth 2.0
        static public async Task<string> GetAccessToken()
        {
            using (var httpClient = new HttpClient())
            {
                var url = "https://login.microsoftonline.com/<YOUR_TENTANT_ID>/oauth2/token";
                var values = new Dictionary<string, string>
                {
                    { "client_id", "<YOUR_CLIENT_ID>" },
                    { "client_secret", "<YOUR_CLIENT_SECRET>" },
                    { "resource", "https://storage.azure.com" },
                    { "grant_type", "client_credentials" }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                Dictionary<string, string> tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                tokens.TryGetValue("access_token", out string accessToken);

                return accessToken ?? string.Empty;
            }
        }
        #endregion

        #region HTTPClient
        static async Task<List<string>> ListBlobsAsync(string containerUrl, string token, string msVersion)
        {
            var blobUrls = new List<string>();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("x-ms-version", msVersion);
                var response = await client.GetAsync(containerUrl + "?restype=container&comp=list"); //https://learn.microsoft.com/en-us/rest/api/storageservices/list-containers2?tabs=microsoft-entra-id

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(responseString);
                    foreach (System.Xml.XmlNode blob in xmlDoc.GetElementsByTagName("Blob"))
                    {
                        var nameNode = blob["Name"];
                        if (nameNode != null)
                        {
                            string blobName = nameNode.InnerText;
                            string blobUrl = $"{containerUrl}{blobName}";
                            blobUrls.Add(blobUrl);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error listing blobs: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            return blobUrls;
        }
        private static async Task DownloadLargeFile(string url, string destinationPath, string token, string msVersion)
        {
            long defaultChunkSize = 50 * 1024; // 50 KB
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("x-ms-version", msVersion);

                var initialResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                if (!initialResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error getting file size: {initialResponse.StatusCode}");
                    return;
                }

                var fileSize = initialResponse.Content.Headers.ContentLength.GetValueOrDefault();
                var numberOfChunks = (int)Math.Ceiling((double)fileSize / defaultChunkSize);

                string filename = "";
                if (initialResponse.Content.Headers.ContentDisposition != null)
                {
                    filename = initialResponse.Content.Headers.ContentDisposition.FileName.Trim('"');
                }
                else
                {
                    Uri uri = new Uri(url);
                    filename = Path.GetFileName(uri.LocalPath);
                }

                Directory.CreateDirectory(destinationPath);

                using (var fs = File.Create(Path.Combine(destinationPath, filename)))
                {
                    long offset = 0;

                    for (int i = 0; i < numberOfChunks; i++)
                    {
                        long chunkSize = Math.Min(defaultChunkSize, fileSize - offset);
                        var req = new HttpRequestMessage(HttpMethod.Get, url);
                        req.Headers.Range = new RangeHeaderValue(offset, offset + chunkSize - 1);

                        var response = await client.SendAsync(req);
                        if (response.IsSuccessStatusCode)
                        {
                            using (var rs = await response.Content.ReadAsStreamAsync())
                            {
                                byte[] buffer = new byte[chunkSize];
                                int read;

                                while ((read = await rs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fs.WriteAsync(buffer, 0, read);
                                }
                                Console.WriteLine($"Downloaded chunk {i + 1}/{numberOfChunks}: {filename}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Error downloading chunk {i + 1}: {response.StatusCode}");
                            return;
                        }
                        offset += chunkSize;
                    }
                }
            }
        }
        #endregion

        static async Task Main()
        {
            try
            {
                string token = await GetAccessToken();
                string containerUrl = "<YOUR_CONTAINER_URL>";
                string destinationPath = "<YOUR_DESTINATION_PATH>";
                string msVersion = "2020-04-08";

                var blobUrls = await ListBlobsAsync(containerUrl, token, msVersion);

                foreach (var blobUrl in blobUrls)
                {
                    await DownloadLargeFile(blobUrl, destinationPath, token, msVersion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }
    }
}
