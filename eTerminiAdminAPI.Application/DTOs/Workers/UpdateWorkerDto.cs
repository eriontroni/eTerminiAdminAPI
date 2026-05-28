namespace eTerminiAdminAPI.Application.DTOs.Workers;

public class UpdateWorkerDto
{
    public string  FirstName   { get; set; } = string.Empty;
    public string  LastName    { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string  Title       { get; set; } = string.Empty;
}
