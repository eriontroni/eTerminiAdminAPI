using eTerminiAdminAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/departments")]
public class DepartmentsAdminController : ControllerBase
{
    private readonly IAdminDepartmentService _service;

    public DepartmentsAdminController(IAdminDepartmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByInstitution([FromQuery] Guid institutionId)
        => Ok(await _service.GetByInstitutionAsync(institutionId));
}
