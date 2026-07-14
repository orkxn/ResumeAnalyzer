namespace ResumeAnalyzer.ViewModels 
{
    public class ResumeListViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int? Score { get; set; } 
    }
}