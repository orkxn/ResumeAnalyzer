using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace ResumeAnalyzer.Validators
{
    public class ResumeFileValidator : AbstractValidator<IFormFile>
    {
        public ResumeFileValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Lütfen geçerli bir dosya seçin.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Length)
                        .GreaterThan(0).WithMessage("Yüklenen dosya boş olamaz.")
                        .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("Dosya boyutu en fazla 5MB olabilir.");

                    RuleFor(x => x.FileName)
                        .Must(fileName =>
                        {
                            if (string.IsNullOrEmpty(fileName)) return false;
                            var ext = Path.GetExtension(fileName).ToLower();
                            return new[] { ".pdf", ".doc", ".docx" }.Contains(ext);
                        }).WithMessage("Sadece PDF veya Word (.doc, .docx) dosyaları yükleyebilirsiniz.");
                });
        }
    }
}
