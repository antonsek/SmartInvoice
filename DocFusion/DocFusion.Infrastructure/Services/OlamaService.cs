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
        _model = config["Ollama:Model"] ?? "llama3.1";
    }
    public async Task<string> ProcessAsync(string text, string prompt)
    {
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "Ты помощник. Отвечай строго по запросу пользователя." },
                new { role = "user", content = $"{prompt}\n\nТекст документа:\n{text}"}
            },
            stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/generate", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return result?.message?.content ?? "";
    }

    

    
}
