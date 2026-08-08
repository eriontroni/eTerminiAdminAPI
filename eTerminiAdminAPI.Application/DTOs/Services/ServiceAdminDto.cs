namespace eTerminiAdminAPI.Application.DTOs.Services;

public class ServiceAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
