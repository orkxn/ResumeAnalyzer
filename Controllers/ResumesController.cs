using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ResumeAnalyzer.Services.Interface;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    public class ResumesController : Controller
    {
        private readonly IResumeService _resumeService;

        public ResumesController(IResumeService resumeService)
        {
            _resumeService = resumeService;
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
        public async Task<IActionResult> Upload(IFormFile resumeFile)
        {
            if (resumeFile == null || resumeFile.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen geçerli bir dosya yükleyin.");
                return View();
            }

            // Backend Dosya Uzantısı Doğrulaması
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(resumeFile.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("", "Sadece PDF veya Word (.doc, .docx) dosyaları yükleyebilirsiniz.");
                return View();
            }

            try
            {
                var resultDto = await _resumeService.ProcessUploadAsync(
                    resumeFile, CurrentUserId, HttpContext.RequestAborted);

                return RedirectToAction(nameof(Details), new { id = resultDto.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"İşlem sırasında bir hata oluştu: {ex.Message}");
                return View();
            }
        }

        // CV Detaylarını (Analiz sonuçlarını) gösterecek sayfa
        public async Task<IActionResult> Details(int id)
        {
            var resume = await _resumeService.GetResumeByIdAsync(id, HttpContext.RequestAborted);
            if (resume == null)
            {
                return NotFound();
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