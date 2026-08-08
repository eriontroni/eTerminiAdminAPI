namespace eTerminiAdminAPI.Application.DTOs.Services;

public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 30;
}
