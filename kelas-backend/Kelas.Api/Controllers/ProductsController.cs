using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kelas.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductListResponse>>> GetAll([FromQuery] string? search)
    {
        var result = await _service.GetAllAsync(search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailResponse>> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailResponse>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDetailResponse>> Update(
        string id, [FromBody] UpdateProductRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id}/recipe")]
    public async Task<ActionResult<ProductDetailResponse>> UpdateRecipe(
        string id, [FromBody] UpdateRecipeRequest request)
    {
        var result = await _service.UpdateRecipeAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id}/visibility")]
    public async Task<ActionResult<ProductDetailResponse>> UpdateVisibility(
        string id, [FromBody] UpdateVisibilityRequest request)
    {
        var result = await _service.UpdateVisibilityAsync(id, request);
        return Ok(result);
    }
}
