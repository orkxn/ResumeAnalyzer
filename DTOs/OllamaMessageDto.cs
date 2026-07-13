using System.Text.Json.Serialization;

namespace ResumeAnalyzer.DTOs;

public class OllamaMessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!; // "system" veya "user"

    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}