# Resume Analyzer (Özgeçmiş Analiz)

An ASP.NET Core MVC web application that analyzes PDF/Word resumes using an LLM (via Ollama) and stores parsed records in SQL Server.

Bu proje, PDF ve Word formatındaki özgeçmişleri (CV) yerel veya bulut tabanlı bir dil modeli (Ollama) kullanarak analiz eden ve sonuçları SQL Server veritabanında saklayan bir ASP.NET Core MVC web uygulamasıdır.

---

## English

### Features
- Resume Parsing: Extracts text content from uploaded PDF, DOC, and DOCX files.
- AI-Powered Analysis: Scores resumes, highlights strengths, identifies weaknesses, and provides actionable suggestions using local LLM models.
- Double-Language support: Adapts prompts dynamically to analyze and respond in the detected language of the resume (Turkish or English).
- User Authentication: Built-in registration, login, and secure user session management.
- Password Reset: Full forgot password and reset password flows with secure SMTP email delivery.
- Resume Comparison: Allows comparing two analyzed resumes side-by-side.
- Modern UI: Responsive design built with Flowbite and Lucide Icons, supporting light and dark themes.
- Security & Optimization: Immediate undoable delete mechanism, server-side pagination, rate-limiting on forms, and process isolation for environment credentials.
- Containerization: Full Docker and Docker Compose orchestration support.

### Technologies
- Framework: .NET 10.0 (ASP.NET Core MVC)
- Database: Microsoft SQL Server (Entity Framework Core)
- Text Extraction: UglyToad.PdfPig (for PDF) and DocumentFormat.OpenXml (for Word)
- AI Endpoint: Ollama API (via HttpClient)
- Styling & UI: HTML5, CSS3, Tailwind CSS, Flowbite, Lucide Icons
- Containerization: Docker, Docker Compose

### Setup and Running

#### 1. Configuration
Create a `.env` file in the root directory and configure the environment variables as follows:
```env
DATABASE_CONNECTION_STRING=YourSQLServerConnectionString
OLLAMA_BASEURL=http://localhost:11434
OLLAMA_MODELNAME=your-model-name
GOOGLE_DRIVE_CLIENT_ID=your-google-drive-client-id
GOOGLE_DRIVE_CLIENT_SECRET=your-google-drive-client-secret
GOOGLE_DRIVE_REFRESHTOKEN=your-google-drive-refresh-token
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=your-sender-email@gmail.com
SMTP_PASSWORD=your-app-password
```

#### 2. Running Locally (Directly)
Run the migrations to initialize the database:
```bash
dotnet ef database update
```
Build and run the project:
```bash
dotnet build
dotnet run
```

#### 3. Running with Docker Compose (Full Containerization)
Build and start both the application and SQL Server database containers:
```bash
docker-compose up -d
```
The application will be accessible at `http://localhost:8080`.

#### 4. Running Database on Docker, Application Locally
If you want to run the database in a container but run the application locally using `dotnet watch`:
```bash
docker-compose up -d db
dotnet watch
```

---

## Türkçe

### Özellikler
- Özgeçmiş Metin Çıkarımı: Yüklenen PDF, DOC ve DOCX dosyalarından metin içeriğini ayrıştırır.
- Yapay Zeka Destekli Analiz: Özgeçmişi puanlar, güçlü yanlarını ortaya çıkarır, zayıf yönlerini belirler ve Ollama üzerinden çalışan dil modeli aracılığıyla aday için somut öneriler sunar.
- Çok Dilli Yapı: Özgeçmişin dilini algılar ve analizi otomatik olarak o dilde (Türkçe veya İngilizce) gerçekleştirir.
- Kullanıcı Yönetimi: Kayıt olma, giriş yapma ve güvenli oturum yönetimi sunar.
- Şifre Sıfırlama: Google SMTP altyapısı ile entegre e-posta doğrulamalı şifremi unuttum akışı.
- CV Karşılaştırma: Seçilen iki özgeçmişin analiz sonuçlarını yan yana karşılaştırır.
- Modern Arayüz: Flowbite bileşenleri ve Lucide ikon kütüphanesiyle donatılmış, açık ve koyu tema modlarını destekleyen tasarım.
- Performans ve Güvenlik: Geri alınabilir anlık silme yapısı, sayfa başına 6 ögeli sunucu tabanlı sayfalama, form istek sınırlayıcıları ve çevre değişkenleri izolasyonu.
- Konteynerleştirme: Tam Docker ve Docker Compose desteği.

### Teknolojiler
- Çatı: .NET 10.0 (ASP.NET Core MVC)
- Veritabanı: Microsoft SQL Server (Entity Framework Core)
- Metin Çözümleme: PDF dosyaları için UglyToad.PdfPig, Word belgeleri için DocumentFormat.OpenXml
- Yapay Zeka: Ollama API (HttpClient entegrasyonu)
- Arayüz: HTML5, CSS3, Tailwind CSS, Flowbite, Lucide Icons
- Konteynerleştirme: Docker, Docker Compose

### Kurulum ve Çalıştırma

#### 1. Yapılandırma
Kök dizinde bir `.env` dosyası oluşturup aşağıdaki gibi yapılandırın:
```env
DATABASE_CONNECTION_STRING=SizinSQLServerBaglantiCümleniz
OLLAMA_BASEURL=http://localhost:11434
OLLAMA_MODELNAME=model-adiniz
GOOGLE_DRIVE_CLIENT_ID=google-drive-client-id
GOOGLE_DRIVE_CLIENT_SECRET=google-drive-client-secret
GOOGLE_DRIVE_REFRESHTOKEN=google-drive-refresh-token
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=gonderici-posta-adresiniz@gmail.com
SMTP_PASSWORD=uygulama-sifreniz
```

#### 2. Doğrudan Yerelde Çalıştırma
Veritabanını oluşturmak için migrasyonları uygulayın:
```bash
dotnet ef database update
```
Projeyi derleyin ve çalıştırın:
```bash
dotnet build
dotnet run
```

#### 3. Docker Compose ile Konteyner Olarak Çalıştırma
Uygulamayı ve SQL Server veritabanını Docker üzerinde ayağa kaldırın:
```bash
docker-compose up -d
```
Uygulamaya `http://localhost:8080` adresinden erişebilirsiniz.

#### 4. Sadece Veritabanını Docker'da, Projeyi Yerelde Çalıştırma
SQL Server'ı bilgisayarınıza kurmadan Docker üzerinde çalıştırmak, web uygulamasını ise `dotnet watch` ile yerelde koşturmak için:
```bash
docker-compose up -d db
dotnet watch
```
