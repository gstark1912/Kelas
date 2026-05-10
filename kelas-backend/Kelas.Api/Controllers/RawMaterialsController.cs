using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/raw-materials")]
[Authorize]
public class RawMaterialsController : ControllerBase
{
    private readonly IRawMaterialService _service;

    public RawMaterialsController(IRawMaterialService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<RawMaterialListResponse>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? unit)
    {
        var result = await _service.GetAllAsync(search, unit);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RawMaterialDetailResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RawMaterialDetailResponse>> Create(
        [FromBody] CreateRawMaterialRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RawMaterialDetailResponse>> Update(
        string id, [FromBody] UpdateRawMaterialRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
