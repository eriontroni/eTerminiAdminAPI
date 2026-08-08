using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Services;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/services")]
public class ServicesAdminController : ControllerBase
{
    private readonly IAdminServiceService _service;

    public ServicesAdminController(IAdminServiceService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Services.View)]
    public async Task<IActionResult> GetByInstitution(
        [FromQuery] Guid institutionId,
        [FromQuery] bool includeInactive = false)
        => Ok(await _service.GetByInstitutionAsync(institutionId, includeInactive));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Services.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try { return Ok(await _service.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost]
    [HasPermission(Permissions.Services.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        var (success, message, svc) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(svc);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Services.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
    {
        var (success, message, svc) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(svc);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Services.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Services.CreateUpdate)]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var (success, message, svc) = await _service.ToggleActiveAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(svc);
    }
}
