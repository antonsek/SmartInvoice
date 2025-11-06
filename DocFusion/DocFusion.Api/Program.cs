using DocFusion.Application.Interfaces;
using DocFusion.Application.Services;
using DocFusion.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
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
builder.Services.AddTransient<DocumentValidationService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddSingleton<IOcrService>(new OcrService());
builder.Services.AddHttpClient<IAiService, OllamaAiService>();
builder.Services.AddControllers();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 100 MB
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();