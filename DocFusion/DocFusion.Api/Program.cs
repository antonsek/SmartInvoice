using DocFusion.Application.Interfaces;
using DocFusion.Application.Services;
using DocFusion.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DocFusion API",
        Version = "v1",
        Description = "OCR + AI document extraction service"
    });
});
builder.Services.AddTransient<ExtractDataService>();
builder.Services.AddSingleton<IOcrService>(new OcrService());
builder.Services.AddControllers();

//windows C:\Program Files\Tesseract-OCR\tessdata

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();