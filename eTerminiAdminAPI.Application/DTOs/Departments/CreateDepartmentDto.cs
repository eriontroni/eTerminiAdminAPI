namespace eTerminiAdminAPI.Application.DTOs.Departments;

public class CreateDepartmentDto
{
    public string  Name          { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public Guid    InstitutionId { get; set; }
}
