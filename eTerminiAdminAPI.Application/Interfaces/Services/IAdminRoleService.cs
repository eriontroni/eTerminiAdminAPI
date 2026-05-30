using eTerminiAdminAPI.Application.DTOs.Roles;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminRoleService
{
    Task<IEnumerable<RoleDto>>                   GetAllAsync();
    Task<RoleDto?>                               GetByIdAsync(Guid id);
    Task<(bool Success, string Message, RoleDto? Role)> CreateAsync(CreateRoleDto dto);
    Task<(bool Success, string Message, RoleDto? Role)> UpdateAsync(Guid id, UpdateRoleDto dto);
    Task<(bool Success, string Message)>         DeleteAsync(Guid id);
    IEnumerable<PermissionCatalogModuleDto>      GetPermissionCatalog();
}
