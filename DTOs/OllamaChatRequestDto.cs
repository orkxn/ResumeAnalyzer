using System.Text.Json.Serialization;

namespace ResumeAnalyzer.DTOs;

public class OllamaChatRequestDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public List<OllamaMessageDto> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false; // Yanıtı tek seferde (JSON) almak için false yapıyoruz

    [JsonPropertyName("format")]
    public string? Format { get; set; } // Çıktıyı zorunlu olarak JSON formatında almak için "json" göndereceğiz
}