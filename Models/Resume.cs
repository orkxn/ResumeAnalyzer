using System.ComponentModel.DataAnnotations;

namespace ResumeAnalyzer.Models;

public class Resume : BaseEntity
{
    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    
    [Required, MaxLength(255)]
    public string FileName { get; set; } = null!;
    
    public string? RawText { get; set; } // NVARCHAR(MAX)
    
    public string? GoogleDriveFileId { get; set; }
    public string? GoogleDriveFileUrl { get; set; }
    
    public string? ParsedJson { get; set; }
    
    public Analysis? Analysis { get; set; }
}