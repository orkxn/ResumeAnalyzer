using System;
using System.Collections.Generic;

namespace ResumeAnalyzer.ViewModels 
{
    public class ResumeDetailsViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public string? GoogleDriveFileUrl { get; set; }
        public AnalysisDetailsViewModel? Analysis { get; set; }
    }

    public class AnalysisDetailsViewModel
    {
        public int Score { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string? ModelUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}