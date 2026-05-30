namespace eTerminiAdminAPI.Application.DTOs.Administrators;

public class AdministratorDto
{
    public Guid     Id          { get; set; }
    public string   FullName    { get; set; } = string.Empty;
    public string   Email       { get; set; } = string.Empty;
    public string?  PhoneNumber { get; set; }
    public bool     IsActive    { get; set; }
    public bool     IsSuperAdmin { get; set; }
    public Guid?    RoleId      { get; set; }
    public string   RoleName    { get; set; } = string.Empty;
    public DateTime CreatedAt   { get; set; }
}
