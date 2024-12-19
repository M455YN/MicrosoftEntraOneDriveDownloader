# MicrosoftEntraOneDriveDownloader

## Description
The **MicrosoftEntraOneDriveDownloader** program is a .NET application designed to interact with **Microsoft Azure Blob Storage using OAuth 2.0 authentication**. The program performs the following tasks:

1. **Authenticate using OAuth 2.0**:
 	- Requests an access token from Azure Active Directory using a client ID and secret.
 	- The token is used for all subsequent API calls to authenticate requests.
2. **List blobs (files) in a container**:
	 - Retrieves the list of blobs from a specified Azure Blob Storage container.
	 - Parses the response and extracts blob names and URLs.
3. **Download large files in chunks**:
	- Downloads each blob from the container.
	- Handles large files by dividing them into manageable chunks and writing each chunk to the destination file.
	- Supports HTTP range headers to efficiently download files in parts.

## Features
- **OAuth 2.0 Authentication**: Secures API calls by obtaining an access token from Azure AD.
- **Blob Listing**: Lists all files (blobs) in the specified Azure Blob Storage container.
- **Chunked File Downloads**: Handles large files by downloading them in chunks to avoid memory issues.
- **Customizable Parameters**: Allows configuration of container URLs, destination paths, and Azure API version.
- **Error Handling**: Captures and logs errors during authentication, blob listing, or file download operations.

## How It Works
1. **Authenticate**:
	- The GetAccessToken method sends a POST request to Azure AD to retrieve an access token using client credentials.
	- The token is included in the Authorization header for subsequent requests.
2. **List Blobs**:
	- The ListBlobsAsync method sends a GET request to the Azure Blob Storage REST API to retrieve the list of blobs in a container.
	- Parses the XML response to extract blob names and generate their URLs.
3. **Download Files**:
	- The DownloadLargeFile method downloads each blob from the container in chunks.
	- Uses HTTP range headers to fetch specific byte ranges of the file.
	- Writes each chunk sequentially to a file in the specified destination directory.

## Prerequisites
- .NET SDK (for building and running the program)
- A valid Azure subscription
- An Azure Blob Storage account with the following:
	- Container URL
	- Client ID
	- Client secret

## Configuration
To configure the program, update the following values in the code:
- Azure AD OAuth Settings:
```
var values = new Dictionary<string, string>
{
    { "client_id", "<YOUR_CLIENT_ID>" },
    { "client_secret", "<YOUR_CLIENT_SECRET>" },
    { "resource", "https://storage.azure.com" },
    { "grant_type", "client_credentials" }
};
```
- Container URL and Destination Path:
```
string containerUrl = "<YOUR_CONTAINER_URL>";
string destinationPath = "<YOUR_DESTINATION_PATH>";
```
- Azure API Version:
```
string msVersion = "2020-04-08";
```
## Usage
Build and run the program using your preferred .NET IDE (e.g., Visual Studio) or command-line tools.

The program will:
- Authenticate with Azure AD.
- List all blobs in the specified container.
- Download each blob to the specified local directory.
### Example Output:
```
Downloaded chunk 1/4: samplefile.txt
Downloaded chunk 2/4: samplefile.txt
Downloaded chunk 3/4: samplefile.txt
Downloaded chunk 4/4: samplefile.txt
```
## References
- [Azure Storage REST API Documentation](https://learn.microsoft.com/en-us/rest/api/storageservices/ "Azure Storage REST API Documentation")
- [Microsoft Authentication Library (MSAL)](https://learn.microsoft.com/en-us/entra/identity-platform/msal-overview "Microsoft Authentication Library (MSAL)")
