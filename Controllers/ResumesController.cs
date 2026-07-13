using Microsoft.AspNetCore.Mvc;

namespace CVAnaliz.Controllers // Kendi projenin namespace'i ile değiştirmeyi unutma
{
    public class ResumesController : Controller
    {
        // Kullanıcının mevcut özgeçmişlerini listeleyeceği sayfa
        public IActionResult Index()
        {
            // TODO (Backend): Veritabanından kullanıcıya ait CV'leri getir.
            return View();
        }

        // Yeni CV yükleme sayfasını (Form) gösteren Action
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // Formdan gelen CV'yi karşılayan Action
        [HttpPost]
        public IActionResult Upload(IFormFile resumeFile)
        {
            // TODO (Backend): Gelen dosyayı Google Drive'a yükle, metni oku ve LLM'e gönder.
            // İşlem bitince şimdilik listeleme sayfasına yönlendiriyoruz.
            if (resumeFile != null && resumeFile.Length > 0)
            {
                // Geçici olarak Index'e yönlendirme
                return RedirectToAction(nameof(Index)); 
            }
            
            ModelState.AddModelError("", "Lütfen geçerli bir dosya yükleyin.");
            return View();
        }

        // CV Detaylarını (Analiz sonuçlarını) gösterecek sayfa
        public IActionResult Details(int id)
        {
            // TODO (Backend): ID'ye göre CV analiz sonucunu veritabanından getir.
            return View();
        }

        // Çoklu CV karşılaştırma sayfası
        public IActionResult Compare()
        {
            // TODO (Backend): Seçilen CV'lerin kıyaslama mantığını kur.
            return View();
        }
    }
}