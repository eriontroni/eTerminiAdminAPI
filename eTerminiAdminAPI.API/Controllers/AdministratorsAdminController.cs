using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Administrators;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/administrators")]
public class AdministratorsAdminController : ControllerBase
{
    private readonly IAdminAdministratorService _service;

    public AdministratorsAdminController(IAdminAdministratorService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Administrators.View)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [HasPermission(Permissions.Administrators.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorDto dto)
    {
        var (success, message, admin) = await _service.CreateAsync(dto);
        return success ? Ok(admin) : BadRequest(new { message });
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Administrators.CreateUpdate)]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var (success, message) = await _service.ToggleActiveAsync(id);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}
