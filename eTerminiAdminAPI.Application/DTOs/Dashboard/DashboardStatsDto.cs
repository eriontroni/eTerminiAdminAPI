namespace eTerminiAdminAPI.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int InstitutionCount  { get; set; }
    public int WorkerCount       { get; set; }
    public int AppointmentCount  { get; set; }
    public int ActiveUserCount   { get; set; }
    public int TodayAppointments { get; set; }
    public IEnumerable<RecentActivityDto> RecentActivity { get; set; } = [];
}

public class RecentActivityDto
{
    public string   Action   { get; set; } = string.Empty;
    public string   Entity   { get; set; } = string.Empty;
    public string?  EntityId { get; set; }
    public DateTime At       { get; set; }
}
