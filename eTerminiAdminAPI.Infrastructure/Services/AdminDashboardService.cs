using eTerminiAdminAPI.Application.DTOs.Dashboard;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _uow;

    public AdminDashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var institutions = await _uow.Institutions.GetAllAsync();
        var staff        = await _uow.StaffMembers.GetAllAsync();
        var appointments = await _uow.Appointments.GetAllAsync();
        var users        = await _uow.Users.GetAllAsync();
        var logs         = await _uow.AuditLogs.GetAllAsync();

        var today = DateTime.UtcNow.Date;
        var todayAppointments = appointments.Count(a =>
            a.AppointmentDate.HasValue && a.AppointmentDate.Value.Date == today);

        var recentActivity = logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => new RecentActivityDto
            {
                Action   = l.Action,
                Entity   = l.EntityName,
                EntityId = l.EntityId,
                At       = l.CreatedAt
            });

        return new DashboardStatsDto
        {
            InstitutionCount  = institutions.Count(),
            WorkerCount       = staff.Count(),
            AppointmentCount  = appointments.Count(),
            ActiveUserCount   = users.Count(u => u.IsActive),
            TodayAppointments = todayAppointments,
            RecentActivity    = recentActivity
        };
    }
}
