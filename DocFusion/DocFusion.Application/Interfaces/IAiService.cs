namespace DocFusion.Application.Interfaces;

public interface IAiService
{
    Task<string> ProcessAsync(string text, string prompt);
    Task<string> CustomProcessAsync(string text, string prompt);
}