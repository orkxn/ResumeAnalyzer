namespace ResumeAnalyzer.DTOs;

public class AnalysisResponseDto
{
    public int Id { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public int Score { get; set; }
    public string? ModelUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}