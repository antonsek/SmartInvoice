namespace DocFusion.Api.Models;

public class ExtractRequest
{
    public IFormFile File { get; set; }
    public string Prompt { get; set; }
}