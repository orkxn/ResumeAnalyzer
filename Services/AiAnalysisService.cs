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
        var language = DetectLanguage(resumeText);
        string systemPrompt;
        string userPrompt;

        if (language == "tr")
        {
            systemPrompt = @"# ROL
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
    4. Netlik ve Öz Anlatım (%15): Gereksiz uzun cümleler, klişe ifadeler yerine net ve kanıta dayalı anlatım.
    5. Profesyonel Sunum (%10): Dil bilgisi hataları, yazım tutarlılığı, iletişim bilgilerinin eksiksizliği.

    # KURALLAR
    1. 'score' ve 'atsCompatibility.score' değerleri 0-100 arasında birer TAM SAYI olmalıdır. Ondalık kullanma.
    2. Tüm metin alanları TÜRKÇE ve profesyonel bir dille yazılmalıdır.
    3. 'strengths' ve 'weaknesses' listelerinde en az 2, en fazla 5 madde olmalı. Genel geçer ifadeler yerine CV'deki somut bilgilere referans ver (örn: ""3 yıllık .NET deneyimi net şekilde belirtilmiş"" gibi; sadece ""deneyimli"" deme).
    4. 'suggestions' listesindeki her öneri, adayın doğrudan uygulayabileceği somut bir eylem içermeli (örn: ""Proje bölümüne kullanılan teknolojileri ve elde edilen ölçülebilir sonucu ekleyin"").
    5. Eğer gönderilen metin bir özgeçmiş değilse veya analiz edilemeyecek kadar yetersizse (ör. boş, anlamsız veya alakasız metin), score değerini 0 ver, diğer tüm liste alanlarını boş dizi ([]) olarak döndür ve 'summary' alanında bunun bir özgeçmiş olmadığını Türkçe olarak belirt.
    6. Asla var olmayan bilgi uydurma (hallüsinasyon yapma); yalnızca metinde geçen veya metinden makul şekilde çıkarılabilen bilgilere dayan.
    7. Aşırı cömert veya aşırı sert puanlama yapma; kriterlere sadık, tutarlı ve gerçekçi bir değerlendirme yap.
    8. YANIT DİLİ KESİNLİKLE VE TAMAMEN TÜRKÇE OLMALIDIR.";

            userPrompt = $"[ZORUNLU TALİMAT]: Lütfen aşağıdaki özgeçmişi analiz et ve KESİNLİKLE %100 TÜRKÇE bir JSON yanıt üret. Kesinlikle İngilizce hiçbir kelime, cümle veya açıklama yazma.\n\nÖzgeçmiş Metni:\n{resumeText}";
        }
        else
        {
            systemPrompt = @"# ROLE
    You are a senior HR Director and Resume (CV) Analysis expert with 15 years of experience.
    Your task is to analyze the provided resume text objectively, consistently, and according to professional criteria.

    # OUTPUT FORMAT (MANDATORY)
    Your response must be ONLY a valid JSON object matching the schema below.
    - Do not write any text outside of the JSON (no explanation, introduction, or markdown block wrapping).
    - Do not use markdown code block tags like ```json, ```.
    - The output must be pure JSON so it can be parsed directly.

    # JSON SCHEMA
    {
      ""score"": 85,
      ""summary"": ""A brief 2-3 sentence summary of the candidate's profile"",
      ""strengths"": [""Strength 1"", ""Strength 2"", ""Strength 3""],
      ""weaknesses"": [""Weakness 1"", ""Weakness 2""],
      ""suggestions"": [""Actionable suggestion 1"", ""Actionable suggestion 2""],
      ""missingElements"": [""Missing standard section or detail in CV""],
      ""atsCompatibility"": {
        ""score"": 70,
        ""notes"": ""Short note on ATS compatibility""
      }
    }

    # EVALUATION CRITERIA
    Consider the following weighted criteria when calculating the 'score':
    1. Content and Experience Quality (35%): Work experiences described with concrete, measurable results (numbers, percentages, projects).
    2. Structure and Readability (20%): Logical ordering of sections, consistent date formats, absence of redundant repetitions.
    3. Keyword and Competency Fit (20%): Technical and soft skills suitable for the industry/position are specified.
    4. Clarity and Conciseness (15%): Clear, evidence-based descriptions instead of overly long sentences or clichés.
    5. Professional Presentation (10%): Grammatical accuracy, spelling consistency, completeness of contact information.

    # RULES
    1. 'score' and 'atsCompatibility.score' must be integers between 0 and 100. Do not use decimals.
    2. All text fields must be in ENGLISH and written in a professional tone.
    3. 'strengths' and 'weaknesses' lists must have between 2 and 5 items. Refer to concrete info in the CV.
    4. Each suggestion must contain a concrete action the candidate can apply directly.
    5. If the sent text is not a resume or is insufficient for analysis, score must be 0, list fields empty arrays ([]), and state in 'summary' that it is not a resume.
    6. Never hallucinate information; base everything only on text present or reasonably deducible.
    7. Do not be overly generous or harsh; be realistic and consistent.
    8. THE LANGUAGE OF THE ENTIRE RESPONSE MUST BE STRICLY ENGLISH.";

            userPrompt = $"[MANDATORY INSTRUCTION]: Please analyze the following resume and produce a response strictly 100% in ENGLISH. Do not use Turkish or any other language.\n\nResume Text:\n{resumeText}";
        }

        var requestBody = new
        {
            model = _modelName,
            format = "json", // Ollama'nın JSON modunu aktif ediyoruz
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
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
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new FlexibleIntConverter());
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

    private string DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "tr";

        // Türkçe'ye özgü harfler ve yaygın Türkçe kelimelerin varlığına bakarız
        string[] turkishIndicators = { "ı", "ğ", "ü", "ş", "ö", "ç", "İ", "Ğ", "Ü", "Ş", "Ö", "Ç", " ve ", " bir ", " için ", " ile ", " olarak ", " mezun ", " tecrübe ", " deneyim " };
        foreach (var indicator in turkishIndicators)
        {
            if (text.Contains(indicator, StringComparison.OrdinalIgnoreCase))
            {
                return "tr";
            }
        }
        return "en";
    }
}

public class FlexibleIntConverter : System.Text.Json.Serialization.JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int val))
            {
                return val;
            }
            return (int)reader.GetDouble();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return 0;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var c in stringValue)
            {
                if (char.IsDigit(c) || c == '-')
                {
                    sb.Append(c);
                }
            }
            var cleaned = sb.ToString();
            if (int.TryParse(cleaned, out int result))
            {
                return result;
            }
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}