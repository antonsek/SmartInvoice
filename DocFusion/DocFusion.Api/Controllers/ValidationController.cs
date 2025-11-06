using DocFusion.Api.Models;
using DocFusion.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocFusion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly DocumentValidationService _documentValidationService;
    private readonly ValidationService _validationService;
    public ValidationController(DocumentValidationService documentValidationService, ValidationService validationService)
    {
        _documentValidationService = documentValidationService;
        _validationService = validationService;
    }

    [HttpPost("Invoice")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> InvoiceCheck([FromForm] ExtractRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("Файл не загружен.");

        await using var stream = request.File.OpenReadStream();

        var result = await _documentValidationService.InvoiceCheckAsync(
            stream, 
            request.File.FileName
        );

        return Ok(result);
    }
    
    [HttpPost("Contract")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ContractCheck([FromForm] ExtractRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("Файл не загружен.");

        await using var stream = request.File.OpenReadStream();

        var result = await _documentValidationService.ContractCheckAsync(
            stream, 
            request.File.FileName
        );

        return Ok(result);
    }
    
    [HttpPost("ProductPurpose")]
    public async Task<IActionResult> ProductPurposeCheck(string purpose)
    {
        if (purpose.Length == 0)
            return BadRequest("нет текста");

        var result = await _validationService.ProductPurposeCheckAsync(
            purpose
        );

        return Ok(result);
    }
    
    [HttpPost("ServicePurpose")]
    public async Task<IActionResult> ServicePurpose(string purpose)
    {
        if (purpose.Length == 0)
            return BadRequest("нет текста");

        var result = await _validationService.ServicePurposeCheckAsync(
            purpose
        );

        return Ok(result);
    }
}