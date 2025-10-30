using DocFusion.Application.Interfaces;
using Tesseract;

namespace DocFusion.Infrastructure.Services;

public class TesseractOcrService : IOcrService
{
    private readonly string _tessDataPath;

    public TesseractOcrService(string tessDataPath)
    {
        _tessDataPath = tessDataPath;
    }

    public string ExtractText(Stream fileStream, string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ExtractFromPdf(fileStream),
            _ => ExtractFromImage(fileStream)
        };
    }

    private string ExtractFromImage(Stream stream)
    {
        using var engine = new TesseractEngine(_tessDataPath, "eng+rus", EngineMode.Default);
        using var tempFile = new TempFile(stream);
        using var img = Pix.LoadFromFile(tempFile.Path);

        using var page = engine.Process(img);
        Console.WriteLine($"🖼️ OCR Confidence: {page.GetMeanConfidence() * 100:0.0}%");
        return page.GetText();
    }

    private string ExtractFromPdf(Stream pdfStream)
    {
        return "";
    }
    
    // --- вспомогательные классы ---
    private sealed class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile(Stream input)
        {
            Path = System.IO.Path.GetTempFileName();
            using var fs = File.Create(Path);
            input.CopyTo(fs);
            fs.Flush();
        }

        public void Dispose()
        {
            try { File.Delete(Path); } catch { }
        }
    }
}

