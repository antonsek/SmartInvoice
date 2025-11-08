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
                        Отвечай строго в формате JSON без комментариев и описаний.

                        ---

                        ### Задача

                        Тебе дан текст документа (на русском или английском языке), полученный после OCR.

                        Определи вероятность (в процентах), что документ является **ИНВОЙСОМ / СЧЁТОМ / BILL / СЧЕТОМ-ФАКТУРОЙ**.

                        ---

                        ### Ключевые признаки инвойса:

                        1. **invoiceNumber** — номер документа.  
                           Должен содержать буквы или цифры, например: `0009`, `INV-2024-15`, `Счет №1234`.  
                           Если значение — случайные символы (`asdf`, `---`, `123abc123abc`), оно считается некорректным.

                        2. **invoiceDate / dueDate** — даты выставления и оплаты.  
                           Должны быть в реальном формате даты, например: `27.11.2016`, `02 December 2016`.

                        3. **supplierData** — данные поставщика:  
                           название, ИИН/БИН/ИНН, адрес, страна, контакты.

                        4. **buyerData** — данные покупателя:  
                           название, адрес, страна, реквизиты.

                        5. **totalSum** — сумма счета (число + возможная валюта, например: `446.40 GBP`, `125000 KZT`, `500 USD`).  
                           Если только число без валюты, вероятность ниже.  
                           Если нет числа — вероятность ≤ 30.

                        6. **itemsTable** — таблица товаров или услуг (название, количество, цена, сумма).  
                           Примеры: “Ultrabook ASUS 380.00”, “Оплата услуг перевозки”, “A4 Paper 10 pcs”.  
                           Если просто случайные слова без цен — считать отсутствующей.

                        7. **invoiceWords** — наличие слов, указывающих на инвойс:  
                           `Invoice`, `Счёт`, `Счет-фактура`, `Bill`, `Supplier`, `Buyer`, `Subtotal`, `VAT`, `Total`, `Payment terms`.

                        ---

                        ### Логика оценки вероятности

                        - Если найдено **5–7 признаков** и текст явно содержит слова `Invoice`, `Счёт`, `Bill` → `Probability = 85–100`.
                        - Если найдено **3–4 признака** → `Probability = 50–80`.
                        - Если найдено **1–2 признака** → `Probability = 20–40`.
                        - Если документ похож на **контракт, акт, соглашение, заявление, договор** → `Probability ≤ 15`.
                        - Если документ содержит **только валюту или сумму, но нет структуры инвойса** → `Probability ≤ 10`.

                        ---

                        ### Проверка корректности данных

                        - Даты — похожи на реальные (`дд.мм.гггг` или `Month DD, YYYY`).  
                        - Суммы — содержат цифры и валюту (не просто “123”).  
                        - Компании — содержат осмысленные слова, а не аббревиатуры из случайных букв.  
                        - Таблица товаров — минимум 2 колонки со значениями (название + цена).  
                        - Если данные выглядят бессмысленно — считать поле отсутствующим.

                        ---

                        ### Итог

                        Верни результат строго в формате JSON:

                        {
                          ""Probability"": <int>
                        }
                        Без текста, пояснений и комментариев.
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
    
    public async Task<DocumentCheckResult> ContractCheckAsync(Stream fileStream, string fileName)
    {
        var text = _ocrService.ExtractText(fileStream, fileName);
        var json = await _aiService.CustomProcessAsync(text,
            @"
                       Ты — эксперт по юридическим и финансовым документам.
                        Отвечай строго в формате JSON без пояснений, комментариев или текста вне JSON.

                        ---

                        ### Задача

                        Тебе дан текст документа после OCR (на русском или английском языке).

                        Определи вероятность (в процентах), что это **валютный контракт / договор / соглашение**  
                        (то есть документ, регулирующий поставку или оказание услуг с оплатой в иностранной валюте).

                        ---

                        ### Ключевые признаки валютного контракта:

                        1. **contractNumber** — номер контракта (должен содержать цифры и/или буквы, например “№15/2024”, “Contract No. 25”).
                        2. **contractDate** — дата заключения (в формате даты, например “10.03.2024”, “March 10, 2024”).
                        3. **supplierData** — данные продавца (название компании, адрес, страна, регистрационные данные — БИН/ИНН/ИИН/ОГРН и т.п.).
                        4. **buyerData** — данные покупателя (аналогично: название, адрес, страна, регистрационные данные).
                        5. **contractSum** — сумма контракта (число + валюта, например “125000 USD”, “3 500 EUR”, “1 000 000 KZT”).
                        6. **currencyCode** — корректный ISO код валюты (USD, EUR, KZT, GBP, CNY, RUB и т.д.).
                        7. **terms** — наличие текстов разделов или пунктов с условиями договора:  
                           «Срок действия», «Обязанности сторон», «Расчёты», «Ответственность»,  
                           «Delivery terms», «Payment terms», «Liability», «Parties agree» и т.п.

                        ---

                        ### Логика оценки вероятности

                        - Если найдено **5–7 признаков** — `Probability = 80–100`
                        - Если найдено **3–4 признака** — `Probability = 40–70`
                        - Если найдено **1–2 признака** — `Probability = 10–30`
                        - Если документ содержит слова:  
                          `Invoice`, `Счёт`, `Bill`, `Receipt`, `Платёж`, `Акт`, `Statement`,  
                          но **нет признаков контракта или соглашения**,  
                          → `Probability <= 15`

                        Если встречаются слова вроде  
                        «Договор», «Контракт», «Соглашение», «Agreement», «Contract»,  
                        но нет даты, номера или условий — вероятность должна быть умеренной (30–60%).

                        ---

                        ### Проверка корректности данных:

                        - Для сумм — должны быть **цифры + валюта**, а не просто число.  
                        - Для даты — должна быть похожа на реальную дату (не “0000-00-00”).  
                        - Для контрагентов — должно быть реальное название компании и адрес.  
                        - Если данные выглядят как случайные символы, числа или не имеют смысла — считать их отсутствующими.

                        ---

                        ### Формат ответа

                        Ответ верни строго в формате JSON:

                        {
                          ""Probability"": <int>
                        }

                        Без текста, пояснений и комментариев."
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