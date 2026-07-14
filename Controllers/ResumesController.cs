using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Services.Interface;
using ResumeAnalyzer.ViewModels;

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
            string currentUserId = "test-user-orkun";
            
            var resumes = await _context.Resumes
                .Include(r => r.Analysis) 
                .Where(r => r.UserId == currentUserId)
                .Select(r => new ResumeListViewModel
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    CreatedAt = r.CreatedAt,
                    Score = r.Analysis != null ? r.Analysis.Score : null 
                })
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
    }
}