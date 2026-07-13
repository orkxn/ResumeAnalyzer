using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ResumeAnalyzer.DTOs;

public class ResumeUploadRequestDto
{
    [Required(ErrorMessage = "Lütfen bir özgeçmiş dosyası seçin.")]
    public IFormFile File { get; set; } = null!;
}