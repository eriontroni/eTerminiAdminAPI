using eTerminiAdminAPI.Application.DTOs.Institutions;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminInstitutionService : IAdminInstitutionService
{
    private readonly IUnitOfWork _uow;

    public AdminInstitutionService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<InstitutionAdminDto>> GetAllAsync()
    {
        var institutions = (await _uow.Institutions.GetAllAsync()).ToList();
        var branches     = (await _uow.InstitutionBranches.GetAllAsync()).ToList();
        var departments  = (await _uow.Departments.GetAllAsync()).ToList();
        var staff        = (await _uow.StaffMembers.GetAllAsync()).ToList();

        var deptToInst = departments.ToDictionary(d => d.Id, d => d.InstitutionId);

        return institutions.Select(i => new InstitutionAdminDto
        {
            Id          = i.Id,
            TenantId    = i.TenantId,
            Name        = i.Name,
            Description = i.Description,
            City        = i.City,
            Address     = i.Address,
            PhoneNumber = i.PhoneNumber,
            Email       = i.Email,
            LogoUrl     = i.LogoUrl,
            IsActive    = i.IsActive,
            CreatedAt   = i.CreatedAt,
            BranchCount = branches.Count(b => b.InstitutionId == i.Id),
            WorkerCount = staff.Count(s => deptToInst.TryGetValue(s.DepartmentId, out var iid) && iid == i.Id)
        }).OrderBy(i => i.Name);
    }

    public async Task<InstitutionAdminDto> GetByIdAsync(Guid id)
    {
        var institution = await _uow.Institutions.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Institucioni me ID {id} nuk u gjet.");

        var branches = await _uow.InstitutionBranches.FindAsync(b => b.InstitutionId == id);
        var depts    = await _uow.Departments.FindAsync(d => d.InstitutionId == id);
        var deptIds  = depts.Select(d => d.Id).ToHashSet();
        var staff    = await _uow.StaffMembers.FindAsync(s => deptIds.Contains(s.DepartmentId));

        return new InstitutionAdminDto
        {
            Id          = institution.Id,
            TenantId    = institution.TenantId,
            Name        = institution.Name,
            Description = institution.Description,
            City        = institution.City,
            Address     = institution.Address,
            PhoneNumber = institution.PhoneNumber,
            Email       = institution.Email,
            LogoUrl     = institution.LogoUrl,
            IsActive    = institution.IsActive,
            CreatedAt   = institution.CreatedAt,
            BranchCount = branches.Count(),
            WorkerCount = staff.Count()
        };
    }

    public async Task<InstitutionAdminDto> CreateAsync(CreateInstitutionDto dto)
    {
        var institution = new Institution
        {
            Id          = Guid.NewGuid(),
            TenantId    = dto.TenantId,
            Name        = dto.Name,
            Description = dto.Description,
            City        = dto.City,
            Address     = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Email       = dto.Email,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        await _uow.Institutions.AddAsync(institution);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(institution.Id);
    }

    public async Task<InstitutionAdminDto> UpdateAsync(Guid id, UpdateInstitutionDto dto)
    {
        var institution = await _uow.Institutions.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Institucioni me ID {id} nuk u gjet.");

        institution.Name        = dto.Name;
        institution.Description = dto.Description;
        institution.City        = dto.City;
        institution.Address     = dto.Address;
        institution.PhoneNumber = dto.PhoneNumber;
        institution.Email       = dto.Email;
        institution.UpdatedAt   = DateTime.UtcNow;

        _uow.Institutions.Update(institution);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<InstitutionAdminDto> ToggleActiveAsync(Guid id)
    {
        var institution = await _uow.Institutions.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Institucioni me ID {id} nuk u gjet.");

        institution.IsActive  = !institution.IsActive;
        institution.UpdatedAt = DateTime.UtcNow;

        _uow.Institutions.Update(institution);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }
}
