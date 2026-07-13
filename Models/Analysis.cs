using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models;

public class Analysis : BaseEntity
{
    public int ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;
    
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    
    public int Score { get; set; }
    
    [MaxLength(100)]
    public string? ModelUsed { get; set; }
}