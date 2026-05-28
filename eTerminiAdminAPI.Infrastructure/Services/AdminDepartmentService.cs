using eTerminiAdminAPI.Application.DTOs.Departments;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminDepartmentService : IAdminDepartmentService
{
    private readonly IUnitOfWork _uow;

    public AdminDepartmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DepartmentDto>> GetByInstitutionAsync(Guid institutionId)
    {
        var departments = await _uow.Departments.FindAsync(d => d.InstitutionId == institutionId && d.IsActive);
        return departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                Id            = d.Id,
                Name          = d.Name,
                InstitutionId = d.InstitutionId,
                IsActive      = d.IsActive
            });
    }
}
