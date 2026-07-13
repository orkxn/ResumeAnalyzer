using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services.Interface;

namespace ResumeAnalyzer.Controllers;

public class HomeController : Controller
{
    private readonly IResumeService _resumeService;

    public HomeController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TestUpload(ResumeUploadRequestDto requestDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            // İleride Identity bağlandığında: User.FindFirstValue(ClaimTypes.NameIdentifier)
            var result = await _resumeService.ProcessUploadAsync(
                requestDto.File, "test-user-orkun", HttpContext.RequestAborted);

            return Json(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"İşlem sırasında bir hata oluştu: {ex.Message}");
        }
    }
}