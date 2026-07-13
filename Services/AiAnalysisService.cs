using System.Text;
using System.Text.Json;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services.Interface;

namespace ResumeAnalyzer.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl;
    private readonly string _modelName;

    public AiAnalysisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _ollamaUrl = $"{configuration["Ollama:BaseUrl"] ?? "http://localhost:11434"}/api/chat";
        _modelName = configuration["Ollama:ModelName"] ?? "llama3:latest";
    }

    public async Task<AnalysisResponseDto> AnalyzeResumeAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"Sen profesyonel bir İK ve Özgeçmiş Analiz uzmanısın. 
    Sana gönderilen özgeçmiş metnini analiz etmeli ve SADECE aşağıda belirtilen JSON formatında yanıt dönmelisin.
    JSON haricinde hiçbir açıklama, markdown işareti (```json gibi) veya giriş/gelişme cümlesi yazma.

    İstenen JSON Formatı:
    {
    ""score"": 85,
    ""strengths"": [""Güçlü yön 1"", ""Güçlü yön 2""],
    ""weaknesses"": [""Zayıf yön 1"", ""Zayıf yön 2""],
    ""suggestions"": [""Öneri 1"", ""Öneri 2""]
    }

    Kurallar:
    1. 'score' değeri 0-100 arasında bir tam sayı olmalıdır.
    2. Tüm metinler Türkçe olmalıdır.";

        var requestBody = new OllamaChatRequestDto
        {
            Model = _modelName,
            Format = "json", // Ollama'nın JSON modunu aktif ediyoruz
            Stream = false,
            Messages = new List<OllamaMessageDto>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = $"İşte analiz edilecek özgeçmiş metni:\n\n{resumeText}" }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_ollamaUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Ollama'dan dönen chat yanıt sarmalını çözüyoruz
        using var doc = JsonDocument.Parse(jsonResponse);
        var aiMessageContent = doc.RootElement.GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrEmpty(aiMessageContent))
            throw new InvalidOperationException("Ollama'dan boş yanıt döndü.");

        // AI'ın ürettiği iç JSON'ı bizim AnalysisResponseDto modeline eşliyoruz
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var analysisResult = JsonSerializer.Deserialize<AnalysisResponseDto>(aiMessageContent, options);

        if (analysisResult == null)
            throw new InvalidOperationException("AI yanıtı beklenen JSON formatına dönüştürülemedi.");

        analysisResult.ModelUsed = _modelName;
        analysisResult.CreatedAt = DateTime.UtcNow;

        return analysisResult;
    }
}