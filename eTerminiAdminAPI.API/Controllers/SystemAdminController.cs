using eTerminiAdminAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTerminiAdminAPI.API.Controllers;

[ApiController]
[Route("api/admin/system")]
public class SystemAdminController : ControllerBase
{
    private readonly IAdminSystemService _service;

    public SystemAdminController(IAdminSystemService service) => _service = service;

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(await _service.GetLogsAsync(page, pageSize));

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() => Ok(await _service.GetUsersAsync());
}
