using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _service;

    public PurchasesController(IPurchaseService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseResponse>> Create(
        [FromBody] CreatePurchaseRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetByRawMaterial), null, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchaseResponse>>> GetByRawMaterial(
        [FromQuery] string? rawMaterialId)
    {
        if (string.IsNullOrWhiteSpace(rawMaterialId))
            throw new BusinessException("El filtro por materia prima es obligatorio.");

        var result = await _service.GetByRawMaterialAsync(rawMaterialId);
        return Ok(result);
    }
}
