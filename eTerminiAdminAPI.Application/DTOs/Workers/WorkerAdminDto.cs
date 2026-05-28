namespace eTerminiAdminAPI.Application.DTOs.Workers;

public class WorkerAdminDto
{
    public Guid     Id              { get; set; }
    public Guid     UserId          { get; set; }
    public string   FullName        { get; set; } = string.Empty;
    public string   Email           { get; set; } = string.Empty;
    public string?  PhoneNumber     { get; set; }
    public string   Title           { get; set; } = string.Empty;
    public bool     IsActive        { get; set; }
    public Guid     DepartmentId    { get; set; }
    public string   DepartmentName  { get; set; } = string.Empty;
    public Guid     InstitutionId   { get; set; }
    public string   InstitutionName { get; set; } = string.Empty;
    public DateTime CreatedAt       { get; set; }
}
