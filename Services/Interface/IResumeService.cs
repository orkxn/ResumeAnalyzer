using Microsoft.AspNetCore.Http;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Services.Interface;

public interface IResumeService
{
    /// <summary>
    /// CV yükleme akışının tamamını yönetir:
    /// Metin çıkarma → AI analiz → DB kayıt → Drive yükleme → DB güncelleme
    /// </summary>
    Task<ResumeResponseDto> ProcessUploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);

    Task<List<Resume>> GetUserResumesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Resume?> GetResumeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteResumeAsync(int id, string userId, CancellationToken cancellationToken = default);
}
