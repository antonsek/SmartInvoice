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

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

builder.Services.AddRazorPages();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseStaticFiles();

app.MapRazorPages();
app.MapControllers();

app.Run();