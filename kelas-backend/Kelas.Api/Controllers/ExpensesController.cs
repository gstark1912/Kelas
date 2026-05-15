using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ExpenseListResponse>> Get([FromQuery] ExpenseFilterRequest filter)
    {
        var result = await _service.GetByFiltersAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("by-category")]
    public async Task<ActionResult<List<CategoryTotalResponse>>> GetByCategory([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        if (from == default || to == default)
            return BadRequest(new { error = "Los parámetros 'from' y 'to' son obligatorios." });

        var result = await _service.GetExpensesByCategoryAsync(from, to);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create([FromBody] CreateExpenseRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
