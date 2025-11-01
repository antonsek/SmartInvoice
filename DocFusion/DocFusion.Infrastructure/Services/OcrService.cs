using DocFusion.Application.Interfaces;
using IronOcr;

namespace DocFusion.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IronTesseract _ocr;

    public OcrService()
    {
        _ocr = new IronTesseract();
    }

    public string ExtractText(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ExtractFromPdf(fileStream),
            _ => ExtractFromImage(fileStream)
        };
    }

    private string ExtractFromImage(Stream stream)
    {
        using var input = new OcrInput();
        input.LoadImage(stream);

        var result = _ocr.Read(input);
        Console.WriteLine($"🖼️ OCR Confidence: {result.Confidence:0.0}%");
        return result.Text;
    }

    private string ExtractFromPdf(Stream pdfStream)
    {
        using var input = new OcrInput();
        input.LoadPdf(pdfStream);

        var result = _ocr.Read(input);
        Console.WriteLine($"📄 PDF OCR Confidence: {result.Confidence:0.0}%");
        return result.Text;
    }
}