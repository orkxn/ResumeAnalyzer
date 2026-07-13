namespace ResumeAnalyzer.DTOs;

public class ResumeResponseDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string? GoogleDriveFileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public AnalysisResponseDto? Analysis { get; set; }
}