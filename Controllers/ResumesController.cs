using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ResumeAnalyzer.Services;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.ViewModels;
using Microsoft.AspNetCore.RateLimiting;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [EnableRateLimiting("GeneralPolicy")]
    public class ResumesController : Controller
    {
        private readonly ResumeService _resumeService;

        public ResumesController(ResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Kullanıcının mevcut özgeçmişlerini listeleyeceği sayfa
        public async Task<IActionResult> Index()
        {
            // Servis katmanından kullanıcının özgeçmişlerini al (Analysis dahil, tarihe göre sıralı)
            var resumes = await _resumeService.GetUserResumesAsync(CurrentUserId, HttpContext.RequestAborted);

            // Liste görünümü için ViewModel'e dönüştür
            var viewModels = resumes
                .Select(r => new ResumeListViewModel
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    CreatedAt = r.CreatedAt,
                    Score = r.Analysis?.Score
                })
                .ToList();

            return View(viewModels);
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
            var resume = await _resumeService.GetResumeByIdAsync(id, CurrentUserId);

            if (resume == null)
            {
                return NotFound(); 
            }

            // 2. Mapping: Entity'deki (Resume) verileri, yeni oluşturduğumuz ViewModel'e aktarıyoruz
            var viewModel = new ResumeDetailsViewModel
            {
                FileName = resume.FileName,
                GoogleDriveFileUrl = resume.GoogleDriveFileUrl,
        
                Analysis = resume.Analysis != null ? new AnalysisDetailsViewModel
                {
                    Score = resume.Analysis.Score,
                    Strengths = resume.Analysis.Strengths ?? new List<string>(),
                    Weaknesses = resume.Analysis.Weaknesses ?? new List<string>(),
                    Suggestions = resume.Analysis.Suggestions ?? new List<string>(),
                    ModelUsed = resume.Analysis.ModelUsed,
                    CreatedAt = resume.Analysis.CreatedAt
                } : null
            };

            // 3. Hazırladığımız bu temiz ViewModel'i View'a gönderiyoruz
            return View(viewModel);
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
            // 1. Tüm özgeçmişleri veritabanından çekiyoruz
            var resumesList = await _resumeService.GetUserResumesAsync(CurrentUserId, HttpContext.RequestAborted);

            // 2. Kullanıcı URL'den id göndermişse (seçim yapmışsa) o CV'leri listeden buluyoruz
            var resume1 = resumesList.FirstOrDefault(r => r.Id == id1);
            var resume2 = resumesList.FirstOrDefault(r => r.Id == id2);

            // 3. ViewModel'imizi oluşturup içini doldurmaya başlıyoruz
            var viewModel = new CompareViewModel
            {
                SelectedId1 = id1,
                SelectedId2 = id2,

                AvailableResumes = resumesList.Select(r => new CompareDropdownItem
                {
                    Id = r.Id,
                    FileName = r.FileName
                }).ToList(),

                // Eğer 1. CV seçilmişse, onu ekranda göstermek üzere ViewModel'e dönüştürüyoruz
                Resume1 = resume1 != null
                    ? new ResumeDetailsViewModel
                    {
                        FileName = resume1.FileName,
                        Analysis = resume1.Analysis != null
                            ? new AnalysisDetailsViewModel
                            {
                                Score = resume1.Analysis.Score,
                                Strengths = resume1.Analysis.Strengths ?? new List<string>(),
                                Weaknesses = resume1.Analysis.Weaknesses ?? new List<string>(),
                                Suggestions = resume1.Analysis.Suggestions ?? new List<string>(),
                            }
                            : null
                    }
                    : null,

                // Eğer 2. CV seçilmişse, aynı dönüştürme işlemini onun için de yapıyoruz
                Resume2 = resume2 != null
                    ? new ResumeDetailsViewModel
                    {
                        FileName = resume2.FileName,
                        Analysis = resume2.Analysis != null
                            ? new AnalysisDetailsViewModel
                            {
                                Score = resume2.Analysis.Score,
                                Strengths = resume2.Analysis.Strengths ?? new List<string>(),
                                Weaknesses = resume2.Analysis.Weaknesses ?? new List<string>(),
                                Suggestions = resume2.Analysis.Suggestions ?? new List<string>(),
                            }
                            : null
                    }
                    : null
            };

            // 4. ViewBag YOK! Temiz ViewModel'imizi View'a gönderiyoruz.
            return View(viewModel);
        }
    }
}