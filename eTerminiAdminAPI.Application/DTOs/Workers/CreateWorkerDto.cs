namespace eTerminiAdminAPI.Application.DTOs.Workers;

public class CreateWorkerDto
{
    public string  FirstName    { get; set; } = string.Empty;
    public string  LastName     { get; set; } = string.Empty;
    public string  Email        { get; set; } = string.Empty;
    public string  Password     { get; set; } = string.Empty;
    public string? PhoneNumber  { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public Guid    DepartmentId { get; set; }
    public Guid    TenantId     { get; set; }
}
