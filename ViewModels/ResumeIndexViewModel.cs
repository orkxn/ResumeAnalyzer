using System.Collections.Generic;

namespace ResumeAnalyzer.ViewModels
{
    public class ResumeIndexViewModel
    {
        public List<ResumeListViewModel> Resumes { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
