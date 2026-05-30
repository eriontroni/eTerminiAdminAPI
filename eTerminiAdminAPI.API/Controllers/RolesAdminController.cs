using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Roles;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/roles")]
public class RolesAdminController : ControllerBase
{
    private readonly IAdminRoleService _service;

    public RolesAdminController(IAdminRoleService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Administrators.View)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("permissions-catalog")]
    [HasPermission(Permissions.Administrators.View)]
    public IActionResult GetPermissionCatalog() => Ok(_service.GetPermissionCatalog());

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Administrators.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _service.GetByIdAsync(id);
        return role == null ? NotFound(new { message = "Roli nuk u gjet." }) : Ok(role);
    }

    [HttpPost]
    [HasPermission(Permissions.Administrators.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var (success, message, role) = await _service.CreateAsync(dto);
        return success
            ? CreatedAtAction(nameof(GetById), new { id = role!.Id }, role)
            : BadRequest(new { message });
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Administrators.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var (success, message, role) = await _service.UpdateAsync(id, dto);
        return success ? Ok(role) : BadRequest(new { message });
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Administrators.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (success, message) = await _service.DeleteAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}
