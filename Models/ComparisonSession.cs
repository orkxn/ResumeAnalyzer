using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models;

public class ComparisonSession : BaseEntity
{
    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    
    public List<int> ResumeIds { get; set; } = new();
    
    public string? ComparisonResultJson { get; set; }
}