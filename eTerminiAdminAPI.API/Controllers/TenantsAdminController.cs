using eTerminiAdminAPI.Application.DTOs.Tenants;
using eTerminiAdminAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public class TenantsAdminController : ControllerBase
{
    private readonly IAdminTenantService _service;

    public TenantsAdminController(IAdminTenantService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
