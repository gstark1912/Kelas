using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/stock-adjustments")]
[Authorize]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentService _service;

    public StockAdjustmentsController(IStockAdjustmentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<StockAdjustmentResponse>> CreateAsync(
        [FromBody] CreateStockAdjustmentRequest request)
    {
        var result = await _service.CreateAsync(request);
        return StatusCode(201, result);
    }
}
