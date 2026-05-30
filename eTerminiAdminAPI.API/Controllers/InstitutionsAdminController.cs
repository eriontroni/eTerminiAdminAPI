using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.DTOs.Institutions;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/institutions")]
public class InstitutionsAdminController : ControllerBase
{
    private readonly IAdminInstitutionService _service;

    public InstitutionsAdminController(IAdminInstitutionService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Institutions.View)]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Institutions.View)]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [HasPermission(Permissions.Institutions.CreateUpdate)]
    public async Task<IActionResult> Create([FromBody] CreateInstitutionDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Institutions.CreateUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInstitutionDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Institutions.CreateUpdate)]
    public async Task<IActionResult> ToggleActive(Guid id)
        => Ok(await _service.ToggleActiveAsync(id));
}
