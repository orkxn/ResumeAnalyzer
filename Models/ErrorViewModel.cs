namespace ResumeAnalyzer.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int? StatusCode { get; set; }
    public string? Title { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IconType { get; set; } // e.g. "404", "403", "500"
}