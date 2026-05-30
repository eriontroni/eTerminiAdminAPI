using eTerminiAdminAPI.Application.DTOs.Departments;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminDepartmentService
{
    Task<IEnumerable<DepartmentDto>>                             GetByInstitutionAsync(Guid institutionId, bool includeInactive = false);
    Task<(bool Success, string Message, DepartmentDto? Dept)>    CreateAsync(CreateDepartmentDto dto);
    Task<(bool Success, string Message, DepartmentDto? Dept)>    UpdateAsync(Guid id, UpdateDepartmentDto dto);
    Task<(bool Success, string Message)>                         DeleteAsync(Guid id);
    Task<(bool Success, string Message, DepartmentDto? Dept)>    ToggleActiveAsync(Guid id);
}
