using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Tenants;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/tenants")]
public class TenantsAdminController : ControllerBase
{
    private readonly IAdminTenantService _service;

    public TenantsAdminController(IAdminTenantService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [HasPermission(Permissions.Tenants.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Tenants.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
