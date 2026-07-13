using Microsoft.AspNetCore.Http;
using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services.Interface;

public interface IResumeService
{
    /// <summary>
    /// CV yükleme akışının tamamını yönetir:
    /// Metin çıkarma → AI analiz → DB kayıt → Drive yükleme → DB güncelleme
    /// </summary>
    Task<ResumeResponseDto> ProcessUploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default);
}
