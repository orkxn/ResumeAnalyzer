using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Services;
using ResumeAnalyzer.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ResumeAnalyzer.Controllers;

public class HomeController : Controller
{
    private readonly ResumeService _resumeService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ResumeService resumeService, ILogger<HomeController> logger)
    {
        _resumeService = resumeService;
        _logger = logger;
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
    public async Task<IActionResult> TestUpload(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Lütfen geçerli bir dosya seçin.");

        var result = await _resumeService.ProcessUploadAsync(
            file, "test-user-orkun", HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return BadRequest($"İşlem sırasında bir hata oluştu: {result.ErrorMessage}");
        }

        return Json(result.Data);
    }

    [Route("Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? HttpContext.Response.StatusCode
        };

        if (exceptionHandlerPathFeature != null)
        {
            var exception = exceptionHandlerPathFeature.Error;
            _logger.LogError(exception, "Global Exception Handler: '{Path}' adresinde unhandled hata yakalandı.", exceptionHandlerPathFeature.Path);

            // özel istisna tiplerine göre dostane mesajlar ve HTTP kodları ata
            if (exception is UnauthorizedAccessException)
            {
                model.Title = "Yetkisiz Erişim (403)";
                model.ErrorMessage = "Bu kaynağa erişmek için gerekli izinlere sahip değilsiniz.";
                model.IconType = "403";
                model.StatusCode = 403;
            }
            else if (exception is KeyNotFoundException)
            {
                model.Title = "Kaynak Bulunamadı (404)";
                model.ErrorMessage = "Aradığınız veya talep ettiğiniz kaynak sistemde mevcut değil.";
                model.IconType = "404";
                model.StatusCode = 404;
            }
            else if (exception is ArgumentException || exception is InvalidOperationException)
            {
                model.Title = "Geçersiz İşlem";
                model.ErrorMessage = exception.Message;
                model.IconType = "500";
            }
            else if (exception is HttpRequestException)
            {
                model.Title = "Bağlantı Hatası";
                model.ErrorMessage = "Harici bir servise (AI analiz motoru vb.) bağlanılamadı. Lütfen birkaç dakika sonra tekrar deneyin.";
                model.IconType = "500";
            }
            else if (exception is TaskCanceledException or OperationCanceledException)
            {
                model.Title = "Zaman Aşımı";
                model.ErrorMessage = "İşlem zaman aşımına uğradı. Sunucu şu an yoğun olabilir, lütfen daha sonra tekrar deneyin.";
                model.IconType = "500";
            }
            else if (exception is Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                model.Title = "Veritabanı Hatası";
                model.ErrorMessage = "Verileriniz kaydedilirken bir sorun oluştu. Lütfen tekrar deneyin.";
                model.IconType = "500";
            }
            else
            {
                model.Title = "Sistem Hatası";
                model.ErrorMessage = "İsteğiniz işlenirken beklenmeyen bir sunucu hatası meydana geldi. Sorun teknik ekibe raporlandı.";
                model.IconType = "500";
            }
        }
        else if (statusCode == 404)
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