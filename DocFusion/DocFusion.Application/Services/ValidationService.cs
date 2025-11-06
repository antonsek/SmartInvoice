using System.Text.Json;
using DocFusion.Application.Helper;
using DocFusion.Application.Interfaces;
using DocFusion.Domain;

namespace DocFusion.Application.Services;

public class ValidationService
{
    private readonly IOcrService _ocrService;
    private readonly IAiService _aiService;

    public ValidationService(IOcrService ocrService , IAiService aiService)
    {
        _ocrService = ocrService;
        _aiService = aiService;
    }
    
        public async Task<DocumentCheckResult> ProductPurposeCheckAsync(string purpose)
    {
        var json = await _aiService.CustomProcessAsync(purpose,
            @"
                       You are an expert in analyzing payment descriptions (both in Russian and English).

                        Your task:
                        1. Determine whether the given text represents a payment for goods.
                        2. Evaluate whether the text is meaningful and specific, or just a generic/uninformative description.
                        Do **NOT** include explanations, markdown, code blocks, or extra text before or after JSON.  
                        Do **NOT** wrap it in ```json or any other formatting.

                        Rules for classification:

                        - The presence of the words “goods”, “товар”, “payment”, or “оплата” **alone does not mean** that the payment is for goods.
                          They only describe the type of document, not the actual content.
                          In such cases, confidence must be **very low (0–20%)**.

                        - If the text is short (less than 10 characters), contains only digits, random symbols, or generic phrases like:
                          - ""за товар"", ""оплата"", ""оплата по договору"", ""по счёту"", ""оплата инвойса""
                          - ""for goods"", ""payment"", ""payment for goods"", ""payment for invoice"", ""for order"", ""by contract""
                          **probability = 0–20%**

                        - Only if the text contains **specific product or material names** (examples:  
                          “оплата за доски хвойные 50×100×6000”, “оплата за бетон М300”,  
                          “payment for pine boards 50x100x6000”, “payment for A4 paper”,  
                          “payment for cement M400”)  
                          **probability = 80–100%**

                        - If the text describes **services, work or actions**  
                          (“услуги”, “работы”, “service”, “transportation”, “repair”, “installation”, “consulting”, “rent”, “maintenance”)  
                          **probability = 0–10%**

                        - If the text is meaningless (“123”, “--”, “zzz”), → **probability = 0%.**
                         **probability = 0%.**
                        Return ONLY a valid JSON object.  
                        Do **NOT** include explanations, markdown, code blocks, or extra text before or after JSON.  
                        Do **NOT** wrap it in ```json or any other formatting.  

                        The output format must be exactly:
                        {
                          ""Probability"": <int>
                        }
                        "
            );
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        
        var result = JsonSerializer.Deserialize<DocumentCheckResult>(AiParser.ParseAiResponse(json), options);

        return result;
    }
    
     public async Task<DocumentCheckResult> ServicePurposeCheckAsync(string purpose)
    {
        var json = await _aiService.CustomProcessAsync(purpose,
            @"
                        You are an expert in analyzing payment descriptions (both in Russian and English).
                        Your task:
                        1. Determine whether the given text represents a payment **for services or works**.
                        2. Evaluate whether the text is **meaningful and specific**, or just a generic/uninformative description.
                        Return ONLY a valid JSON object.  
                        Do **NOT** include explanations, markdown, code blocks, or extra text before or after JSON.  
                        Do **NOT** wrap it in ```json or any other formatting.  

                        ### Rules for classification:

                        - If the text is short (less than 5 characters), contains only numbers, random symbols, or generic phrases like:
                          - ""за услуги"", ""оплата"", ""оплата по договору"", ""по счёту"", ""оплата за работы"", ""оплата инвойса""
                          - ""for services"", ""payment"", ""payment for invoice"", ""by contract"", ""for work""
                          — then it is **too generic**.  
                          **Set probability = 0–20%.**

                        - If the text contains **specific descriptions of services or work types**  
                          (for example: “оплата за услуги перевозки груза”, “оплата за монтаж оборудования”,  
                           “оплата за услуги консультации”, “payment for consulting services”,  
                           “payment for freight transportation”, “payment for installation work”),  
                          **Set probability = 80–100%.**

                        - If the text describes **goods or tangible items**  
                          (“товар”, “продукция”, “материал”, “бетон”, “бумага”, “cement”, “boards”, etc.),  
                          this is **not services**.  
                          **Set probability = 0–10%.**

                        - If the text looks meaningless (like “123”, “abc”, “--”, “zzzzz”),  
                          **probability = 0%.**
                        Return ONLY a valid JSON object.  
                        Do **NOT** include explanations, markdown, code blocks, or extra text before or after JSON.  
                        Do **NOT** wrap it in ```json or any other formatting.  

                        {
                          ""Probability"": <int>
                        }
"
            );
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var result = JsonSerializer.Deserialize<DocumentCheckResult>(AiParser.ParseAiResponse(json), options);

        return result;
    }
}