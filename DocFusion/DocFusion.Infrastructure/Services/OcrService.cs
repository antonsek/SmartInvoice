using DocFusion.Application.Interfaces;
using IronOcr;

namespace DocFusion.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IronTesseract _ocr;

    public OcrService()
    {
        IronOcr.License.LicenseKey = "test";
        _ocr = new IronTesseract();
        _ocr.Language = OcrLanguage.Russian;
        _ocr.AddSecondaryLanguage(OcrLanguage.English);
        _ocr.AddSecondaryLanguage(OcrLanguage.Kazakh);
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

        var result = _ocr.ReadDocument(input);
        Console.WriteLine($"Image OCR Confidence: {result.Confidence:0.0}%");
        return result.Text;
    }

    private string ExtractFromPdf(Stream pdfStream)
    {
        using var input = new OcrInput();
        input.LoadPdf(pdfStream);
        
        var result = _ocr.ReadDocument(input);
        Console.WriteLine($"PDF OCR Confidence: {result.Confidence:0.0}%");
        return result.Text;
    }
}