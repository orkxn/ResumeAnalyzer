using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services.Interface;

public interface IGoogleDriveService
{
    Task<GoogleDriveUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string contentType, string userId, CancellationToken cancellationToken = default);
}