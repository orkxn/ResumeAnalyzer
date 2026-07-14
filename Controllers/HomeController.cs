using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services.Interface;
using ResumeAnalyzer.Models;
using System.Diagnostics;

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

    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("Terms")]
    public IActionResult Terms()
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

    [Route("Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode
        };

        if (statusCode == 404)
        {
            model.Title = "Sayfa Bulunamadı (404)";
            model.ErrorMessage = "Aradığınız sayfa kaldırılmış, adı değiştirilmiş veya geçici olarak kullanılamıyor olabilir.";
            model.IconType = "404";
        }
        else if (statusCode == 403)
        {
            model.Title = "Erişim Engellendi (403)";
            model.ErrorMessage = "Bu sayfaya erişim yetkiniz bulunmamaktadır.";
            model.IconType = "403";
        }
        else
        {
            model.Title = "Bir Hata Oluştu";
            model.ErrorMessage = "İsteğiniz işlenirken beklenmeyen bir sorun yaşandı. Lütfen daha sonra tekrar deneyin.";
            model.IconType = "500";
        }

        return View("~/Views/Shared/Error.cshtml", model);
    }
}