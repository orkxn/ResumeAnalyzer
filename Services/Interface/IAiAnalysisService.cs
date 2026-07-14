using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services.Interface;

public interface IAiAnalysisService
{
    /// <summary>
    /// Ham özgeçmiş metnini Ollama üzerinden analiz eder ve yapılandırılmış DTO döner.
    /// </summary>
    Task<AnalysisResponseDto> AnalyzeResumeAsync(string resumeText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Metnin gerçekten bir CV/özgeçmiş olup olmadığını kontrol eder.
    /// </summary>
    Task<bool> IsResumeAsync(string text, CancellationToken cancellationToken = default);
}