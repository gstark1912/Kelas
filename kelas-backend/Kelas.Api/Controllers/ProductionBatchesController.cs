using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/production-batches")]
[Authorize]
public class ProductionBatchesController : ControllerBase
{
    private readonly IProductionService _service;

    public ProductionBatchesController(IProductionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ProductionBatchDetailResponse>> Create([FromBody] CreateProductionBatchRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<ProductionBatchListResultResponse>> Get(
        [FromQuery] string? productId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var result = await _service.GetAsync(productId, dateFrom, dateTo);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionBatchDetailResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }
}
