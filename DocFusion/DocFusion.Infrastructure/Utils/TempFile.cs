namespace DocFusion.Infrastructure.Utils;

public sealed class TempFile : IDisposable
{
    public TempFile(Stream input)
    {
        Path = System.IO.Path.GetTempFileName();
        using var fs = File.Create(Path);
        input.CopyTo(fs);
        fs.Flush();
    }

    public string Path { get; }

    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch
        {
        }
    }
}