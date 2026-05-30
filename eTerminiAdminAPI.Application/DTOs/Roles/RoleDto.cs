namespace eTerminiAdminAPI.Application.DTOs.Roles;

public class RoleDto
{
    public Guid         Id          { get; set; }
    public string       Name        { get; set; } = string.Empty;
    public string?      Description { get; set; }
    public bool         IsSystem    { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int          AdminCount  { get; set; }
    public DateTime     CreatedAt   { get; set; }
}
