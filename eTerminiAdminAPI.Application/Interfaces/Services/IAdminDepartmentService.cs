using eTerminiAdminAPI.Application.DTOs.Departments;

namespace eTerminiAdminAPI.Application.Interfaces.Services;

public interface IAdminDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetByInstitutionAsync(Guid institutionId);
}
