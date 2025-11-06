using System.Net.Http.Json;
using DocFusion.Application.Interfaces;
using DocFusion.Infrastructure.Models;
using Microsoft.Extensions.Configuration;

namespace DocFusion.Infrastructure.Services;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _model;
    
    public OllamaAiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri(config["Ollama:Host"] ?? "http://localhost:11434");
        _model = config["Ollama:Model"] ?? "phi3:mini";
        _http.Timeout = TimeSpan.FromMinutes(5);
    }
    public async Task<string> ProcessAsync(string text, string prompt)
    {
        var request = new
        {
            model = _model,
            prompt = $@"
                Ты сервис извлечения данных.
                Отвечай только JSON. Без текста, комментариев и описаний.
                Если данных нет — ставь null.

                Запрос:
                {prompt}

                Текст документа:
                {text}

                Ответ должен быть строго JSON.
                ".Trim(),
            stream = false,
            temperature = 0,
            stop = new[] { "```", "###", "</" }
        };

        var response = await _http.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        return json?["response"]?.ToString() ?? "";
    }

    

    
}
