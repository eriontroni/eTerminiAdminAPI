using eTerminiAdminAPI.Application.DTOs.Dashboard;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Enums;

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

    public async Task<IEnumerable<ActiveAppointmentDto>> GetActiveAppointmentsAsync()
    {
        var activeStatuses = new[] { AppointmentStatus.Pending, AppointmentStatus.Confirmed };

        var appointments = (await _uow.Appointments.FindAsync(a => activeStatuses.Contains(a.Status))).ToList();
        var users        = (await _uow.Users.GetAllAsync()).ToList();
        var staff        = (await _uow.StaffMembers.GetAllAsync()).ToList();

        // StaffMember → User për emrin e mjekut
        var userMap     = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
        var doctorNames = staff.ToDictionary(
            s => s.Id,
            s => userMap.TryGetValue(s.UserId, out var n) ? n : "—");

        return appointments
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new ActiveAppointmentDto
            {
                Id              = a.Id,
                PatientName     = userMap.TryGetValue(a.UserId, out var p) ? p : "—",
                DoctorName      = a.DoctorId.HasValue && doctorNames.TryGetValue(a.DoctorId.Value, out var d) ? d : null,
                AppointmentDate = a.AppointmentDate,
                Status          = a.Status == AppointmentStatus.Confirmed ? "Konfirmuar" : "Në pritje",
                Notes           = a.Notes
            });
    }
}
