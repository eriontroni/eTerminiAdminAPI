using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Departments;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/departments")]
public class DepartmentsAdminController : ControllerBase
{
    private readonly IAdminDepartmentService _service;

    public DepartmentsAdminController(IAdminDepartmentService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Departments.View)]
    public async Task<IActionResult> GetByInstitution(
        [FromQuery] Guid institutionId,
        [FromQuery] bool includeInactive = false)
        => Ok(await _service.GetByInstitutionAsync(institutionId, includeInactive));

    [HttpPost]
    [HasPermission(Permissions.Departments.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var (success, message, dept) = await _service.CreateAsync(dto);
        if (!success) return BadRequest(new { message });
        return Ok(dept);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Departments.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var (success, message, dept) = await _service.UpdateAsync(id, dto);
        if (!success) return BadRequest(new { message });
        return Ok(dept);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Departments.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Departments.CreateUpdate)]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var (success, message, dept) = await _service.ToggleActiveAsync(id);
        if (!success) return BadRequest(new { message });
        return Ok(dept);
    }
}
