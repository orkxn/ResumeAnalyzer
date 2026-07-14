using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs;
using ResumeAnalyzer.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;

namespace ResumeAnalyzer.Services;

public class ResumeService
{
    private readonly TextExtractorService _textExtractor;
    private readonly GoogleDriveService _driveService;
    private readonly AiAnalysisService _aiService;
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public ResumeService(
        TextExtractorService textExtractor,
        GoogleDriveService driveService,
        AiAnalysisService aiService,
        ApplicationDbContext context,
        IMemoryCache cache)
    {
        _textExtractor = textExtractor;
        _driveService = driveService;
        _aiService = aiService;
        _context = context;
        _cache = cache;
    }

    public async Task<ServiceResult<ResumeResponseDto>> ProcessUploadAsync(IFormFile file, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. PDF/DOCX Metnini Çıkar
            using var textStream = file.OpenReadStream();
            string extractedText = await _textExtractor.ExtractTextAsync(textStream, file.ContentType);

            // 1b. Belgenin gerçekten bir özgeçmiş (CV) olup olmadığını yapay zeka ile denetle
            bool isResume = await _aiService.IsResumeAsync(extractedText, cancellationToken);
            if (!isResume)
            {
                return ServiceResult<ResumeResponseDto>.Failure("Yüklediğiniz dosya geçerli bir özgeçmiş (CV) içeriği barındırmıyor. Lütfen iş deneyimlerinizi veya eğitim bilgilerinizi içeren gerçek bir özgeçmiş yükleyin.");
            }

            // 2. Ollama AI Analizi
            var aiResult = await _aiService.AnalyzeResumeAsync(extractedText, cancellationToken);

            // 3. DB Transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 4. Resume + Analysis entity'lerini oluştur ve tek seferde kaydet
                var resumeEntity = new Resume
                {
                    FileName = file.FileName,
                    RawText = extractedText,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Analysis = new Analysis
                    {
                        Score = aiResult.Score,
                        Strengths = aiResult.Strengths,
                        Weaknesses = aiResult.Weaknesses,
                        Suggestions = aiResult.Suggestions,
                        ModelUsed = aiResult.ModelUsed ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.Resumes.Add(resumeEntity);
                await _context.SaveChangesAsync(cancellationToken);

                // 5. Google Drive'a yükle
                using var driveStream = file.OpenReadStream();
                var driveResult = await _driveService.UploadFileAsync(
                    driveStream, file.FileName, file.ContentType, userId, cancellationToken);

                // 6. Drive bilgilerini DB'ye güncelle
                resumeEntity.GoogleDriveFileId = driveResult.FileId;
                resumeEntity.GoogleDriveFileUrl = driveResult.WebViewLink;
                resumeEntity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                // 7. Transaction'ı onayla
                await transaction.CommitAsync(cancellationToken);

                // Önbelleği temizle
                _cache.Remove($"resumes_list_{userId}");

                // 8. Response DTO oluştur
                var responseDto = new ResumeResponseDto
                {
                    Id = resumeEntity.Id,
                    FileName = resumeEntity.FileName,
                    GoogleDriveFileUrl = resumeEntity.GoogleDriveFileUrl,
                    CreatedAt = resumeEntity.CreatedAt,
                    Analysis = new AnalysisResponseDto
                    {
                        Id = resumeEntity.Analysis.Id,
                        Score = resumeEntity.Analysis.Score,
                        Strengths = resumeEntity.Analysis.Strengths,
                        Weaknesses = resumeEntity.Analysis.Weaknesses,
                        Suggestions = resumeEntity.Analysis.Suggestions,
                        ModelUsed = resumeEntity.Analysis.ModelUsed,
                        CreatedAt = resumeEntity.Analysis.CreatedAt
                    }
                };

                return ServiceResult<ResumeResponseDto>.Success(responseDto);
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (System.Exception ex)
        {
            return ServiceResult<ResumeResponseDto>.Failure(ex.Message);
        }
    }

    public async Task<List<Resume>> GetUserResumesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"resumes_list_{userId}";
        if (!_cache.TryGetValue(cacheKey, out List<Resume>? resumes))
        {
            resumes = await _context.Resumes
                .Include(r => r.Analysis)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

            _cache.Set(cacheKey, resumes, cacheEntryOptions);
        }

        return resumes ?? new List<Resume>();
    }

    public async Task<Resume?> GetResumeByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"resume_detail_{id}_{userId}";
        if (!_cache.TryGetValue(cacheKey, out Resume? resume))
        {
            resume = await _context.Resumes
                .Include(r => r.Analysis)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);

            if (resume != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

                _cache.Set(cacheKey, resume, cacheEntryOptions);
            }
        }

        return resume;
    }

    public async Task DeleteResumeAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
        if (resume == null)
        {
            throw new KeyNotFoundException("Silmek istediğiniz özgeçmiş bulunamadı veya silme yetkiniz yok.");
        }

        _context.Resumes.Remove(resume);
        await _context.SaveChangesAsync(cancellationToken);

        // Önbelleği temizle
        _cache.Remove($"resumes_list_{userId}");
        _cache.Remove($"resume_detail_{id}");
    }
}
