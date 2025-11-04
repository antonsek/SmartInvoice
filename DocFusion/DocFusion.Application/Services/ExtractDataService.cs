using DocFusion.Application.Interfaces;

namespace DocFusion.Application.Services;

public class ExtractDataService
{
    private readonly IOcrService _ocrService;
    private readonly IAiService _aiService;

    public ExtractDataService(IOcrService ocrService , IAiService aiService)
    {
        _ocrService = ocrService;
        _aiService = aiService;
    }

    public async Task<DocumentResult> ExecuteAsync(Stream fileStream, string fileName, string prompt)
    {
        var text = _ocrService.ExtractText(fileStream, fileName);
        var json = await _aiService.ProcessAsync(text, prompt);

        return new DocumentResult
        {
            RawText = text,
            JsonResult = json
        };
    }
}