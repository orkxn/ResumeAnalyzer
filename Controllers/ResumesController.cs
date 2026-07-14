using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ResumeAnalyzer.Services;
using ResumeAnalyzer.Models;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    public class ResumesController : Controller
    {
        private readonly ResumeService _resumeService;
        private readonly IValidator<IFormFile> _fileValidator;

        public ResumesController(ResumeService resumeService, IValidator<IFormFile> fileValidator)
        {
            _resumeService = resumeService;
            _fileValidator = fileValidator;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Kullanıcının mevcut özgeçmişlerini listeleyeceği sayfa
        public async Task<IActionResult> Index()
        {
            var resumes = await _resumeService.GetUserResumesAsync(CurrentUserId, HttpContext.RequestAborted);
            return View(resumes);
        }

        // Yeni CV yükleme sayfasını (Form) gösteren Action
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // Formdan gelen CV'yi karşılayan Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("UploadPolicy")]
        public async Task<IActionResult> Upload(IFormFile resumeFile)
        {
            var validationResult = await _fileValidator.ValidateAsync(resumeFile, HttpContext.RequestAborted);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return View();
            }

            var serviceResult = await _resumeService.ProcessUploadAsync(
                resumeFile, CurrentUserId, HttpContext.RequestAborted);

            if (!serviceResult.IsSuccess)
            {
                ModelState.AddModelError("", serviceResult.ErrorMessage!);
                return View();
            }

            return RedirectToAction(nameof(Details), new { id = serviceResult.Data!.Id });
        }

        // CV Detaylarını (Analiz sonuçlarını) gösterecek sayfa
        public async Task<IActionResult> Details(int id)
        {
            var resume = await _resumeService.GetResumeByIdAsync(id, CurrentUserId, HttpContext.RequestAborted);
            if (resume == null)
            {
                throw new KeyNotFoundException("İstediğiniz özgeçmiş bulunamadı veya görüntüleme yetkiniz yok.");
            }

            return View(resume);
        }

        // CV Silme işlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _resumeService.DeleteResumeAsync(id, CurrentUserId, HttpContext.RequestAborted);
            return RedirectToAction(nameof(Index));
        }

        // Çoklu CV karşılaştırma sayfası
        [HttpGet]
        public async Task<IActionResult> Compare(int? id1, int? id2)
        {
            var resumesList = await _resumeService.GetUserResumesAsync(CurrentUserId, HttpContext.RequestAborted);

            ViewBag.ResumesList = resumesList;
            ViewBag.Resume1 = resumesList.FirstOrDefault(r => r.Id == id1);
            ViewBag.Resume2 = resumesList.FirstOrDefault(r => r.Id == id2);

            return View();
        }
    }
}