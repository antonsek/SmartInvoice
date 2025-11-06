using System.Text.Json;
using DocFusion.Application.Helper;
using DocFusion.Application.Interfaces;
using DocFusion.Domain;

namespace DocFusion.Application.Services;

public class DocumentValidationService
{
    private readonly IOcrService _ocrService;
    private readonly IAiService _aiService;

    public DocumentValidationService(IOcrService ocrService , IAiService aiService)
    {
        _ocrService = ocrService;
        _aiService = aiService;
    }

    public async Task<DocumentCheckResult> InvoiceCheckAsync(Stream fileStream, string fileName)
    {
        var text = _ocrService.ExtractText(fileStream, fileName);
        var json = await _aiService.CustomProcessAsync(text,
            @"
                        Ты — эксперт по анализу бизнес-документов.
                        Отвечай только JSON. Без текста, комментариев и описаний.
                        Тебе дан текст документа, полученный после OCR. 
                        Твоя задача:
                        1. Определи вероятность (в процентах), что это ИНВОЙС.
                        2. Проверь наличие и корректность ключевых данных:
                           - invoiceNumber — должен содержать только буквы/цифры, не быть случайным набором символов.
                           - supplierData — должен включать осмысленные данные организации: название, ИИН/БИН, адрес или контакт.
                           - buyerData — должен включать осмысленные данные организации или клиента.
                           - totalSum — должна быть числом (с точкой или запятой), возможно с указанием валюты (KZT, USD, EUR и т.п.).
                           - itemsTable — должна содержать список товаров или услуг (название + цена/количество). Простые случайные слова не считаются.
 
                        3. Если значения выглядят бессмысленно (например: ""asdf123"", ""!!!"", ""xyz"", ""none"") — считай, что поле отсутствует или некорректно.

                        4. Верни результат строго в формате JSON, отвечай только JSON. Без текста, комментариев и описаний.:

                        {
                          ""Probability"": <int>
                        }"
            );
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,  
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var result = JsonSerializer.Deserialize<DocumentCheckResult>(AiParser.ParseAiResponse(json), options);

        return result;
    }
    
    public async Task<DocumentCheckResult> ContractCheckAsync(Stream fileStream, string fileName)
    {
        var text = _ocrService.ExtractText(fileStream, fileName);
        var json = await _aiService.CustomProcessAsync(text,
            @"
                        Ты — эксперт по юридическим и финансовым документам.
                        Отвечай только JSON. Без текста, комментариев и описаний.
                        Тебе дан текст документа, полученный после OCR. 
                        Твоя задача:
                        1. Определи вероятность (в процентах), что это валютный контракт (foreign exchange contract, currency agreement, контракт на поставку/услуги с оплатой в иностранной валюте).
                        2. Проверь наличие и корректность ключевых данных:
                        - contractNumber — номер контракта (должен содержать буквы или цифры, не быть абракадаброй).
                        - contractDate — дата заключения (должна быть в формате даты).
                        - supplierData — данные продавца (название, БИН/ИИН, адрес или страна регистрации).
                        - buyerData — данные покупателя (название, БИН/ИИН, адрес или страна регистрации).
                        - contractSum — сумма контракта (число + код валюты, например 125000 USD, 3 500 EUR, 1000000 KZT).
                        - currencyCode — корректный ISO код валюты (USD, EUR, KZT, CNY и т.д.).
                        - terms — наличие раздела с условиями (сроки, поставка, расчёты, ответственность и т.д.).

                        3. Проверяй корректность данных:
                        - если данные выглядят как случайные символы или не содержат реального смысла;
                        - для сумм проверь, что есть цифры и валюта;
                        - для даты проверь, что она похожа на реальную дату;
                        - для контрагентов проверь, что есть название и страна/регистрационные данные.

                        4. Верни результат строго в формате JSON без комментариев и текста:
                        {
                          ""Probability"": <int>
                        }"
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