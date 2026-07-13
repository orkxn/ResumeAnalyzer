namespace ResumeAnalyzer.Services.Interface;

public interface ITextExtractorService
{
    Task<string> ExtractTextAsync(Stream fileStream, string contentType);
}