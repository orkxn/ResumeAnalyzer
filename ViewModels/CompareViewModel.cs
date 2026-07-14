using System.Collections.Generic;

namespace ResumeAnalyzer.ViewModels 
{
    public class CompareViewModel
    {
        public List<CompareDropdownItem> AvailableResumes { get; set; } = new();

        public int? SelectedId1 { get; set; }
        public int? SelectedId2 { get; set; }

        public ResumeDetailsViewModel? Resume1 { get; set; }

        public ResumeDetailsViewModel? Resume2 { get; set; }
    }
    
    public class CompareDropdownItem
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}