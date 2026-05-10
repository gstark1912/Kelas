using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly IStockAdjustmentService _service;

    public StockMovementsController(IStockAdjustmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<StockMovementResponse>>> GetByItemAsync(
        [FromQuery] string itemType,
        [FromQuery] string itemId)
    {
        var result = await _service.GetMovementsByItemAsync(itemType, itemId);
        return Ok(result);
    }
}
