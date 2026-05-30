using eTerminiAdminAPI.API.Authorization;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[HasPermission(Permissions.Dashboard.View)]
public class DashboardAdminController : ControllerBase
{
    private readonly IAdminDashboardService _service;

    public DashboardAdminController(IAdminDashboardService service) => _service = service;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _service.GetStatsAsync());
}
