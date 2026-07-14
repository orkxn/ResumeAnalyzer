using System.Text;
using System.Text.Json;
using ResumeAnalyzer.DTOs;

namespace ResumeAnalyzer.Services;

public class AiAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl;
    private readonly string _modelName;

    public AiAnalysisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _ollamaUrl = $"{configuration["Ollama:BaseUrl"] ?? "http://localhost:11434"}/api/chat";
        _modelName = configuration["Ollama:ModelName"] ?? "llama3:latest";
    }

    public async Task<AnalysisResponseDto> AnalyzeResumeAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        var systemPrompt = @"# ROL
    Sen 15 yıllık deneyime sahip, kıdemli bir İK Direktörü ve Özgeçmiş (CV) Analiz uzmanısın.
    Görevin, sana verilen özgeçmiş metnini objektif, tutarlı ve profesyonel kriterlere göre analiz etmek.

    # ÇIKTI FORMATI (ZORUNLU)
    Yanıtın SADECE ve SADECE aşağıdaki JSON şemasına birebir uyan geçerli bir JSON nesnesi olmalıdır.
    - JSON dışında hiçbir karakter yazma (açıklama, giriş cümlesi, kapanış cümlesi, yorum yok).
    - Markdown kod bloğu işaretleri kullanma (```json, ``` gibi).
    - JSON'un başında veya sonunda boşluk, satır atlama dışında ekstra metin olmasın.
    - Çıktı, doğrudan bir JSON parser'a verilecek şekilde saf olmalı.

    # JSON ŞEMASI
    {
    ""score"": 85,
    ""summary"": ""Adayın genel profilini 2-3 cümlede özetleyen kısa değerlendirme"",
    ""strengths"": [""Güçlü yön 1"", ""Güçlü yön 2"", ""Güçlü yön 3""],
    ""weaknesses"": [""Zayıf yön 1"", ""Zayıf yön 2""],
    ""suggestions"": [""Somut ve uygulanabilir öneri 1"", ""Somut ve uygulanabilir öneri 2""],
    ""missingElements"": [""CV'de eksik olan standart bir bölüm veya bilgi""],
    ""atsCompatibility"": {
        ""score"": 70,
        ""notes"": ""ATS (Aday Takip Sistemi) uyumluluğuna dair kısa not""
    }
    }

    # DEĞERLENDİRME KRİTERLERİ
    'score' alanını hesaplarken aşağıdaki ağırlıklandırılmış kriterleri dikkate al:
    1. İçerik ve Deneyim Kalitesi (%35): İş deneyimlerinin somut, ölçülebilir sonuçlarla (rakamlar, yüzdeler, projeler) anlatılıp anlatılmadığı.
    2. Yapı ve Okunabilirlik (%20): Bölümlerin mantıklı sıralanışı, tutarlı tarih formatları, gereksiz tekrarların olmaması.
    3. Anahtar Kelime ve Yetkinlik Uyumu (%20): Sektöre/pozisyona uygun teknik ve kişisel yetkinliklerin belirtilmiş olması.
    4. Netlik ve Öz Anlatım (%15): Gereksiz uzun cümleler, klişe ifadeler (""takım çalışmasına yatkın"" gibi somut kanıtsız ifadeler) yerine net ve kanıta dayalı anlatım.
    5. Profesyonel Sunum (%10): Dil bilgisi hataları, yazım tutarlılığı, iletişim bilgilerinin eksiksizliği.

    # KURALLAR
    1. 'score' ve 'atsCompatibility.score' değerleri 0-100 arasında birer TAM SAYI olmalıdır. Ondalık kullanma.
    2. Tüm metin alanları TÜRKÇE ve profesyonel bir dille yazılmalıdır.
    3. 'strengths' ve 'weaknesses' listelerinde en az 2, en fazla 5 madde olmalı. Genel geçer ifadeler yerine CV'deki somut bilgilere referans ver (örn: ""3 yıllık .NET deneyimi net şekilde belirtilmiş"" gibi; sadece ""deneyimli"" deme).
    4. 'suggestions' listesindeki her öneri, adayın doğrudan uygulayabileceği somut bir eylem içermeli (örn: ""Proje bölümüne kullanılan teknolojileri ve elde edilen ölçülebilir sonucu ekleyin"").
    5. Eğer gönderilen metin bir özgeçmiş değilse veya analiz edilemeyecek kadar yetersizse (ör. boş, anlamsız veya alakasız metin), score değerini 0 ver, diğer tüm liste alanlarını boş dizi ([]) olarak döndür ve 'summary' alanında bunun bir özgeçmiş olmadığını Türkçe olarak belirt.
    6. Asla var olmayan bilgi uydurma (hallüsinasyon yapma); yalnızca metinde geçen veya metinden makul şekilde çıkarılabilen bilgilere dayan.
    7. Aşırı cömert veya aşırı sert puanlama yapma; kriterlere sadık, tutarlı ve gerçekçi bir değerlendirme yap.";

        var requestBody = new
        {
            model = _modelName,
            format = "json", // Ollama'nın JSON modunu aktif ediyoruz
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"İşte analiz edilecek özgeçmiş metni:\n\n{resumeText}" }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_ollamaUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Ollama'dan dönen chat yanıt sarmalını çözüyoruz
        using var doc = JsonDocument.Parse(jsonResponse);
        var aiMessageContent = doc.RootElement.GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrEmpty(aiMessageContent))
            throw new InvalidOperationException("Ollama'dan boş yanıt döndü.");

        // AI'ın ürettiği iç JSON'ı bizim AnalysisResponseDto modeline eşliyoruz
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var analysisResult = JsonSerializer.Deserialize<AnalysisResponseDto>(aiMessageContent, options);

        if (analysisResult == null)
            throw new InvalidOperationException("AI yanıtı beklenen JSON formatına dönüştürülemedi.");

        analysisResult.ModelUsed = _modelName;
        analysisResult.CreatedAt = DateTime.UtcNow;

        return analysisResult;
    }

    public async Task<bool> IsResumeAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 30)
        {
            return false;
        }

        var systemPrompt = @"Sen bir belgenin Özgeçmiş (CV / Resume) olup olmadığını tespit eden bir yardımcı yazılımsın.
    Sana gönderilen metni analiz et ve bu belgenin gerçek bir özgeçmiş/CV olup olmadığını belirle.
    Bir belgenin özgeçmiş sayılabilmesi için eğitim geçmişi, iş deneyimi, yetenekler, kişisel bilgiler, iletişim detayları veya kariyer hedefi gibi özgeçmişe özgü alanlardan en az birkaçını içermesi gerekir.
    SADECE JSON formatında şu yanıtı dönmelisin:
    {
    ""isResume"": true
    }
    veya 
    {
    ""isResume"": false
    }
    Yanıtında JSON dışında hiçbir metin, açıklama veya markdown bloğu yer almamalıdır.";

        var requestBody = new
        {
            model = _modelName,
            format = "json",
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"İşte analiz edilecek belge metni:\n\n{text}" }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_ollamaUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        
        using var doc = JsonDocument.Parse(jsonResponse);
        var aiMessageContent = doc.RootElement.GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrEmpty(aiMessageContent))
            return false;

        using var innerDoc = JsonDocument.Parse(aiMessageContent);
        if (innerDoc.RootElement.TryGetProperty("isResume", out var isResumeProp))
        {
            return isResumeProp.GetBoolean();
        }

        return false;
    }
}