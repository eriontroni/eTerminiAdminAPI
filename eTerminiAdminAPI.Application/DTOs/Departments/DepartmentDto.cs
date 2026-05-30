namespace eTerminiAdminAPI.Application.DTOs.Departments;

public class DepartmentDto
{
    public Guid    Id             { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public Guid    InstitutionId  { get; set; }
    public bool    IsActive       { get; set; }
}
