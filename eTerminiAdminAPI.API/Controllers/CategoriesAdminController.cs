using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Categories;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
public class CategoriesAdminController : ControllerBase
{
    private readonly IAdminCategoryService _service;

    public CategoriesAdminController(IAdminCategoryService service) => _service = service;

    // Lexim i lirë për çdo admin të autentikuar — përdoret edhe nga forma e institucionit (dropdown).
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cat = await _service.GetByIdAsync(id);
        return cat == null ? NotFound(new { message = "Kategoria nuk u gjet." }) : Ok(cat);
    }

    [HttpPost]
    [HasPermission(Permissions.Categories.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var (success, message, cat) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(cat);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Categories.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var (success, message, cat) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(cat);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Categories.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
