namespace DocFusion.Application.Interfaces;

public interface IOcrService
{
    string ExtractText(Stream fileStream, string fileName);
}