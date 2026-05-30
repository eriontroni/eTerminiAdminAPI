namespace eTerminiAdminAPI.Application.DTOs.Roles;

public class PermissionCatalogModuleDto
{
    public string Key     { get; set; } = string.Empty;
    public string Label   { get; set; } = string.Empty;
    public List<PermissionCatalogActionDto> Actions { get; set; } = new();
}

public class PermissionCatalogActionDto
{
    public string Code  { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
