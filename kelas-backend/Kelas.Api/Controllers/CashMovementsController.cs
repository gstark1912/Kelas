using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/cash-movements")]
[Authorize]
public class CashMovementsController : ControllerBase
{
    private readonly ICashMovementService _service;

    public CashMovementsController(ICashMovementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CashMovementListResponse>> Get([FromQuery] CashMovementFilterRequest filter)
    {
        var result = await _service.GetByFiltersAsync(filter);
        return Ok(result);
    }

    [HttpPost("manual")]
    public async Task<ActionResult<CashMovementResponse>> CreateManual([FromBody] CreateManualCashMovementRequest request)
    {
        var result = await _service.CreateManualAsync(request);
        return Created(string.Empty, result);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<CashTransferResponse>> CreateTransfer([FromBody] CreateCashTransferRequest request)
    {
        var result = await _service.CreateTransferAsync(request);
        return Created(string.Empty, result);
    }
}
