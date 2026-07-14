using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using UglyToad.PdfPig;

namespace ResumeAnalyzer.Services;

public class TextExtractorService
{
    public Task<string> ExtractTextAsync(Stream fileStream, string contentType)
    {
        if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ExtractTextFromPdf(fileStream));
        }
        else if (contentType.Contains("wordprocessingml", StringComparison.OrdinalIgnoreCase) || 
                 contentType.Contains("msword", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ExtractTextFromDocx(fileStream));
        }

        throw new NotSupportedException("Yalnızca PDF ve DOCX formatları desteklenmektedir.");
    }

    private string ExtractTextFromPdf(Stream stream)
    {
        var textBuilder = new StringBuilder();
        
        using (var pdf = PdfDocument.Open(stream))
        {
            foreach (var page in pdf.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
        }

        return textBuilder.ToString().Trim();
    }

    private string ExtractTextFromDocx(Stream stream)
    {
        var textBuilder = new StringBuilder();

        using (var wordDoc = WordprocessingDocument.Open(stream, false))
        {
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart != null && mainPart.Document != null && mainPart.Document.Body is { } body)
            {
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    if (!string.IsNullOrEmpty(paragraph.InnerText))
                    {
                        textBuilder.AppendLine(paragraph.InnerText);
                    }
                }
            }
        }

        return textBuilder.ToString().Trim();
    }
}