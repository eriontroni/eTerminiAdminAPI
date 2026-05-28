using eTerminiAdminAPI.Application.DTOs.Dashboard;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
