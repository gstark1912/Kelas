using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/cash-accounts")]
[Authorize]
public class CashAccountsController : ControllerBase
{
    private readonly ICashAccountService _service;

    public CashAccountsController(ICashAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CashAccountResponse>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<CashAccountSummaryResponse>> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CashAccountResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CashAccountResponse>> Create([FromBody] CreateCashAccountRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
