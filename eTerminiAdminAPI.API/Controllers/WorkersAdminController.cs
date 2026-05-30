using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Workers;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/workers")]
public class WorkersAdminController : ControllerBase
{
    private readonly IAdminWorkerService _service;

    public WorkersAdminController(IAdminWorkerService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Workers.View)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Workers.View)]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [HasPermission(Permissions.Workers.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateWorkerDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Workers.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkerDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Workers.CreateUpdate)]
    public async Task<IActionResult> ToggleActive(Guid id)
        => Ok(await _service.ToggleActiveAsync(id));

    [HttpPatch("{id:guid}/assign-institution")]
    [HasPermission(Permissions.Workers.CreateUpdate)]
    public async Task<IActionResult> AssignInstitution(Guid id, [FromBody] AssignInstitutionDto dto)
        => Ok(await _service.AssignInstitutionAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Workers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "Punëtori u fshi me sukses." });
    }
}
