using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CurriculumAnalyzer.API.Services;

public class FileProcessingService
{
    public async Task<string> ExtractTextAsync(Stream fileStream, string fileType)
    {
        return fileType.ToLower() switch
        {
            "pdf" => ExtractFromPdf(fileStream),
            "docx" => ExtractFromDocx(fileStream),
            "txt" => await ExtractFromTxt(fileStream),
            _ => throw new ArgumentException($"Unsupported file type: {fileType}")
        };
    }

    private string ExtractFromPdf(Stream stream)
    {
        var sb = new StringBuilder();
        using var reader = new PdfReader(stream);
        using var pdfDoc = new PdfDocument(reader);
        var strategy = new SimpleTextExtractionStrategy();
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            sb.AppendLine(PdfTextExtractor.GetTextFromPage(page, strategy));
        }
        return sb.ToString();
    }

    private string ExtractFromDocx(Stream stream)
    {
        var sb = new StringBuilder();
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        foreach (var element in body.Elements())
        {
            if (element is Paragraph paragraph)
            {
                foreach (var run in paragraph.Elements<Run>())
                    foreach (var text in run.Elements<Text>())
                        sb.Append(text.Text);
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private async Task<string> ExtractFromTxt(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
