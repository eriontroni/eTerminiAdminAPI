using eTerminiAdminAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public class DashboardAdminController : ControllerBase
{
    private readonly IAdminDashboardService _service;

    public DashboardAdminController(IAdminDashboardService service) => _service = service;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _service.GetStatsAsync());
}
