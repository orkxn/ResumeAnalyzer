using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Services.Interface;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Controllers
{
    public class ResumesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IResumeService _resumeService;

        public ResumesController(ApplicationDbContext context, IResumeService resumeService)
        {
            _context = context;
            _resumeService = resumeService;
        }

        // Kullanıcının mevcut özgeçmişlerini listeleyeceği sayfa
        public async Task<IActionResult> Index()
        {
            // Şimdilik test kullanıcısının CV'lerini getiriyoruz
            var resumes = await _context.Resumes
                .Include(r => r.Analysis)
                .Where(r => r.UserId == "test-user-orkun")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

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

            try
            {
                // Analiz akışını tetikliyoruz
                var resultDto = await _resumeService.ProcessUploadAsync(
                    resumeFile, "test-user-orkun", HttpContext.RequestAborted);

                // İşlem başarılı olunca Detay sayfasına yönlendiriyoruz
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
            var resume = await _context.Resumes
                .Include(r => r.Analysis)
                .FirstOrDefaultAsync(r => r.Id == id);

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
            var resume = await _context.Resumes.FindAsync(id);
            if (resume != null)
            {
                _context.Resumes.Remove(resume);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Çoklu CV karşılaştırma sayfası
        [HttpGet]
        public async Task<IActionResult> Compare(int? id1, int? id2)
        {
            var resumesList = await _context.Resumes
                .Where(r => r.UserId == "test-user-orkun")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.ResumesList = resumesList;

            Resume? resume1 = null;
            Resume? resume2 = null;

            if (id1.HasValue)
            {
                resume1 = await _context.Resumes
                    .Include(r => r.Analysis)
                    .FirstOrDefaultAsync(r => r.Id == id1.Value && r.UserId == "test-user-orkun");
            }

            if (id2.HasValue)
            {
                resume2 = await _context.Resumes
                    .Include(r => r.Analysis)
                    .FirstOrDefaultAsync(r => r.Id == id2.Value && r.UserId == "test-user-orkun");
            }

            ViewBag.Resume1 = resume1;
            ViewBag.Resume2 = resume2;

            return View();
        }
    }
}