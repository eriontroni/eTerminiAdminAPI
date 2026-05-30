using System.Security.Cryptography;
using System.Text;
using eTerminiAdminAPI.Application.DTOs.Workers;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;
using eTerminiAPI.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminWorkerService : IAdminWorkerService
{
    private readonly IUnitOfWork    _uow;
    private readonly IConfiguration _config;

    public AdminWorkerService(IUnitOfWork uow, IConfiguration config)
    {
        _uow    = uow;
        _config = config;
    }

    public async Task<IEnumerable<WorkerAdminDto>> GetAllAsync()
    {
        var staff        = (await _uow.StaffMembers.GetAllAsync()).ToList();
        var users        = (await _uow.Users.GetAllAsync()).ToDictionary(u => u.Id);
        var departments  = (await _uow.Departments.GetAllAsync()).ToDictionary(d => d.Id);
        var institutions = (await _uow.Institutions.GetAllAsync()).ToDictionary(i => i.Id);

        return staff.Select(s =>
        {
            users.TryGetValue(s.UserId, out var user);
            departments.TryGetValue(s.DepartmentId, out var dept);
            Institution? inst = null;
            if (dept != null) institutions.TryGetValue(dept.InstitutionId, out inst);

            return new WorkerAdminDto
            {
                Id              = s.Id,
                UserId          = s.UserId,
                FullName        = user is null ? string.Empty : $"{user.FirstName} {user.LastName}",
                Email           = user?.Email ?? string.Empty,
                PhoneNumber     = user?.PhoneNumber,
                Title           = s.Title,
                IsActive        = s.IsActive,
                DepartmentId    = s.DepartmentId,
                DepartmentName  = dept?.Name ?? string.Empty,
                InstitutionId   = inst?.Id ?? Guid.Empty,
                InstitutionName = inst?.Name ?? string.Empty,
                CreatedAt       = s.CreatedAt
            };
        }).OrderBy(w => w.FullName);
    }

    public async Task<WorkerAdminDto> GetByIdAsync(Guid id)
    {
        var s = await _uow.StaffMembers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Punëtori me ID {id} nuk u gjet.");

        var users = await _uow.Users.FindAsync(u => u.Id == s.UserId);
        var user  = users.FirstOrDefault();
        var depts = await _uow.Departments.FindAsync(d => d.Id == s.DepartmentId);
        var dept  = depts.FirstOrDefault();
        Institution? inst = null;
        if (dept != null)
        {
            var insts = await _uow.Institutions.FindAsync(i => i.Id == dept.InstitutionId);
            inst = insts.FirstOrDefault();
        }

        return new WorkerAdminDto
        {
            Id              = s.Id,
            UserId          = s.UserId,
            FullName        = user is null ? string.Empty : $"{user.FirstName} {user.LastName}",
            Email           = user?.Email ?? string.Empty,
            PhoneNumber     = user?.PhoneNumber,
            Title           = s.Title,
            IsActive        = s.IsActive,
            DepartmentId    = s.DepartmentId,
            DepartmentName  = dept?.Name ?? string.Empty,
            InstitutionId   = inst?.Id ?? Guid.Empty,
            InstitutionName = inst?.Name ?? string.Empty,
            CreatedAt       = s.CreatedAt
        };
    }

    public async Task<WorkerAdminDto> CreateAsync(CreateWorkerDto dto)
    {
        _ = await _uow.Departments.GetByIdAsync(dto.DepartmentId)
            ?? throw new KeyNotFoundException("Departamenti nuk u gjet.");

        var existing = await _uow.Users.FindAsync(u => u.Email == dto.Email.ToLower().Trim());
        if (existing.Any()) throw new InvalidOperationException("Ky email është tashmë i regjistruar.");

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id           = userId,
            TenantId     = dto.TenantId,
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            Email        = dto.Email.ToLower().Trim(),
            PasswordHash = HashPassword(dto.Password),
            PhoneNumber  = dto.PhoneNumber,
            Role         = UserRole.Staff,
            IsActive     = true,
            IsDeleted    = false,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        var staffMemberId = Guid.NewGuid();
        var staffMember = new StaffMember
        {
            Id           = staffMemberId,
            TenantId     = dto.TenantId,
            UserId       = userId,
            DepartmentId = dto.DepartmentId,
            Title        = dto.Title,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user);
        await _uow.StaffMembers.AddAsync(staffMember);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(staffMemberId);
    }

    public async Task<WorkerAdminDto> UpdateAsync(Guid id, UpdateWorkerDto dto)
    {
        var s = await _uow.StaffMembers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Punëtori me ID {id} nuk u gjet.");

        var users = await _uow.Users.FindAsync(u => u.Id == s.UserId);
        var user  = users.FirstOrDefault()
            ?? throw new KeyNotFoundException("Përdoruesi i lidhur nuk u gjet.");

        user.FirstName   = dto.FirstName;
        user.LastName    = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt   = DateTime.UtcNow;

        s.Title     = dto.Title;
        s.UpdatedAt = DateTime.UtcNow;

        _uow.Users.Update(user);
        _uow.StaffMembers.Update(s);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<WorkerAdminDto> ToggleActiveAsync(Guid id)
    {
        var s = await _uow.StaffMembers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Punëtori me ID {id} nuk u gjet.");

        s.IsActive  = !s.IsActive;
        s.UpdatedAt = DateTime.UtcNow;

        _uow.StaffMembers.Update(s);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var s = await _uow.StaffMembers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Punëtori me ID {id} nuk u gjet.");

        // Soft delete: shëno si i fshirë (global query filter e përjashton nga listat).
        s.IsDeleted = true;
        s.IsActive  = false;
        s.UpdatedAt = DateTime.UtcNow;
        _uow.StaffMembers.Update(s);

        // Fshi edhe llogarinë e përdoruesit të lidhur.
        var users = await _uow.Users.FindAsync(u => u.Id == s.UserId);
        var user  = users.FirstOrDefault();
        if (user != null)
        {
            user.IsDeleted = true;
            user.IsActive  = false;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task<WorkerAdminDto> AssignInstitutionAsync(Guid id, AssignInstitutionDto dto)
    {
        var s = await _uow.StaffMembers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Punëtori me ID {id} nuk u gjet.");

        _ = await _uow.Departments.GetByIdAsync(dto.DepartmentId)
            ?? throw new KeyNotFoundException("Departamenti nuk u gjet.");

        s.DepartmentId = dto.DepartmentId;
        s.UpdatedAt    = DateTime.UtcNow;

        _uow.StaffMembers.Update(s);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private string HashPassword(string password)
    {
        var saltBytes = Encoding.UTF8.GetBytes(_config["Auth:PasswordSalt"]!);
        using var hmac = new HMACSHA256(saltBytes);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}
