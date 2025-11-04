namespace DocFusion.Infrastructure.Models;

public class OllamaResponse
{
    public OllamaMessage? message { get; set; }
}

public class OllamaMessage
{
    public string? content { get; set; }
}