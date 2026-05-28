using eTerminiAdminAPI.Application.DTOs.Workers;
using eTerminiAdminAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/workers")]
public class WorkersAdminController : ControllerBase
{
    private readonly IAdminWorkerService _service;

    public WorkersAdminController(IAdminWorkerService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkerDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkerDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
        => Ok(await _service.ToggleActiveAsync(id));

    [HttpPatch("{id:guid}/assign-institution")]
    public async Task<IActionResult> AssignInstitution(Guid id, [FromBody] AssignInstitutionDto dto)
        => Ok(await _service.AssignInstitutionAsync(id, dto));
}
