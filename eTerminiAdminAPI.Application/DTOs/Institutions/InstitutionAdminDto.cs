namespace eTerminiAdminAPI.Application.DTOs.Institutions;

public class InstitutionAdminDto
{
    public Guid     Id          { get; set; }
    public Guid     TenantId    { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public string   City        { get; set; } = string.Empty;
    public string?  Address     { get; set; }
    public string?  PhoneNumber { get; set; }
    public string?  Email       { get; set; }
    public string?  LogoUrl     { get; set; }
    public bool     IsActive    { get; set; }
    public DateTime CreatedAt   { get; set; }
    public int      BranchCount { get; set; }
    public int      WorkerCount { get; set; }
}
