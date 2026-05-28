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
}
