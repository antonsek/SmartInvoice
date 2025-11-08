using DocFusion.Application.Interfaces;
using IronOcr;

namespace DocFusion.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IronTesseract _ocr;

    public OcrService()
    {
        IronOcr.License.LicenseKey = "IRONSUITE.ANTONSEK96.GMAIL.COM.30010-F8168DE5D8-KZH3Q-6E27HTJ54NT5-LTI4DQ2SFXHR-HWZNVGGZNIUU-5YU3PN3XBANA-W3F4LMWNZCDA-DM526WAEAG4A-SUIX67-TWG55NX4B4OQEA-DEPLOYMENT.TRIAL-D73BKH.TRIAL.EXPIRES.01.DEC.2025";
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