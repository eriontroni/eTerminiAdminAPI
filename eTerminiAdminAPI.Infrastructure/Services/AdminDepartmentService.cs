using eTerminiAdminAPI.Application.DTOs.Departments;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminDepartmentService : IAdminDepartmentService
{
    private readonly IUnitOfWork _uow;

    public AdminDepartmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DepartmentDto>> GetByInstitutionAsync(Guid institutionId, bool includeInactive = false)
    {
        var departments = await _uow.Departments.FindAsync(d =>
            d.InstitutionId == institutionId && (includeInactive || d.IsActive));

        return departments
            .OrderBy(d => d.Name)
            .Select(MapToDto);
    }

    public async Task<(bool Success, string Message, DepartmentDto? Dept)> CreateAsync(CreateDepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i departamentit është i detyrueshëm.", null);

        var institution = await _uow.Institutions.GetByIdAsync(dto.InstitutionId);
        if (institution == null)
            return (false, "Institucioni nuk u gjet.", null);

        var existing = await _uow.Departments.FindAsync(d =>
            d.InstitutionId == dto.InstitutionId && d.Name == dto.Name.Trim());
        if (existing.Any())
            return (false, $"Departamenti '{dto.Name}' ekziston tashmë në këtë institucion.", null);

        var department = new Department
        {
            Id            = Guid.NewGuid(),
            TenantId      = institution.TenantId,
            InstitutionId = dto.InstitutionId,
            Name          = dto.Name.Trim(),
            Description   = dto.Description?.Trim(),
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow
        };

        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();

        return (true, "Departamenti u krijua me sukses.", MapToDto(department));
    }

    public async Task<(bool Success, string Message, DepartmentDto? Dept)> UpdateAsync(Guid id, UpdateDepartmentDto dto)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department == null)
            return (false, "Departamenti nuk u gjet.", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i departamentit është i detyrueshëm.", null);

        var dup = await _uow.Departments.FindAsync(d =>
            d.InstitutionId == department.InstitutionId && d.Name == dto.Name.Trim() && d.Id != id);
        if (dup.Any())
            return (false, $"Departamenti '{dto.Name}' ekziston tashmë në këtë institucion.", null);

        department.Name        = dto.Name.Trim();
        department.Description = dto.Description?.Trim();
        department.UpdatedAt   = DateTime.UtcNow;

        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

        return (true, "Departamenti u përditësua me sukses.", MapToDto(department));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department == null)
            return (false, "Departamenti nuk u gjet.");

        var staffMembers = await _uow.StaffMembers.FindAsync(s => s.DepartmentId == id);
        if (staffMembers.Any())
            return (false, "Ky departament nuk mund të fshihet sepse ka punëtorë të caktuar.");

        _uow.Departments.Delete(department);
        await _uow.SaveChangesAsync();

        return (true, "Departamenti u fshi me sukses.");
    }

    public async Task<(bool Success, string Message, DepartmentDto? Dept)> ToggleActiveAsync(Guid id)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department == null)
            return (false, "Departamenti nuk u gjet.", null);

        department.IsActive  = !department.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

        return (true, $"Departamenti u {(department.IsActive ? "aktivizua" : "çaktivizua")} me sukses.", MapToDto(department));
    }

    private static DepartmentDto MapToDto(Department d) => new()
    {
        Id            = d.Id,
        Name          = d.Name,
        Description   = d.Description,
        InstitutionId = d.InstitutionId,
        IsActive      = d.IsActive
    };
}
