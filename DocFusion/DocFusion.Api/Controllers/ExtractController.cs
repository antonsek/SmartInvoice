using DocFusion.Api.Models;
using DocFusion.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocFusion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtractController : ControllerBase
{
    private readonly ExtractDataService _extractDataService;

    public ExtractController(ExtractDataService extractDataService)
    {
        _extractDataService = extractDataService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] ExtractRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("Файл не загружен.");

        await using var stream = request.File.OpenReadStream();

        var result = await _extractDataService.ExecuteAsync(
            stream, 
            request.File.FileName, 
            request.Prompt
        );

        return Ok(new
        {
            success = true,
            json = result.JsonResult
        });
    }
}