using Microsoft.AspNetCore.Identity;

namespace ResumeAnalyzer.Models;

public class ApplicationUser : IdentityUser
{
    // Kullanıcının sisteme yüklediği özgeçmişler
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    
    // Kullanıcının yaptığı analiz karşılaştırmaları
    public ICollection<ComparisonSession> ComparisonSessions { get; set; } = new List<ComparisonSession>();
}