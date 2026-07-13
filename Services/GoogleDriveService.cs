using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services.Interface;

namespace ResumeAnalyzer.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IConfiguration _configuration;
    private const string RootFolderName = "ResumeAnalyzerFiles";

    public GoogleDriveService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private async Task<DriveService> GetDriveServiceAsync()
    {
        string clientId = _configuration["GoogleDrive:ClientId"] ?? throw new InvalidOperationException("ClientId bulunamadı.");
        string clientSecret = _configuration["GoogleDrive:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret bulunamadı.");
        string refreshToken = _configuration["GoogleDrive:RefreshToken"] ?? throw new InvalidOperationException("RefreshToken bulunamadı.");

        var clientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var tokenResponse = new TokenResponse { RefreshToken = refreshToken };
        
        var credential = new UserCredential(
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = clientSecrets }),
            "user", 
            tokenResponse
        );

        if (credential.Token.IsStale)
        {
            await credential.RefreshTokenAsync(CancellationToken.None);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ResumeAnalyzer"
        });
    }

    public async Task<GoogleDriveUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId, CancellationToken cancellationToken = default)
    {
        var service = await GetDriveServiceAsync();

        string rootFolderId = await GetOrCreateFolderAsync(service, RootFolderName, null);
        string userFolderId = await GetOrCreateFolderAsync(service, userId, rootFolderId);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName,
            Parents = new List<string> { userFolderId }
        };

        FilesResource.CreateMediaUpload request;
        request = service.Files.Create(fileMetadata, fileStream, contentType);
        request.Fields = "id, webViewLink";
        
        await request.UploadAsync(cancellationToken);

        var uploadedFile = request.ResponseBody;
        if (uploadedFile == null)
        {
            throw new InvalidOperationException("Google Drive yüklemesi başarısız oldu, yanıt boş döndü.");
        }

        return new GoogleDriveUploadResultDto
        {
            FileId = uploadedFile.Id,
            WebViewLink = uploadedFile.WebViewLink
        };
    }

    private async Task<string> GetOrCreateFolderAsync(DriveService service, string folderName, string? parentId)
    {
        var listRequest = service.Files.List();
        listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
        if (!string.IsNullOrEmpty(parentId))
        {
            listRequest.Q += $" and '{parentId}' in parents";
        }
        listRequest.Fields = "files(id)";

        var result = await listRequest.ExecuteAsync();
        var folder = result.Files.FirstOrDefault();

        if (folder != null)
        {
            return folder.Id;
        }

        var folderMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        };
        if (!string.IsNullOrEmpty(parentId))
        {
            folderMetadata.Parents = new List<string> { parentId };
        }

        var createRequest = service.Files.Create(folderMetadata);
        createRequest.Fields = "id";
        var newFolder = await createRequest.ExecuteAsync();

        return newFolder.Id;
    }
}