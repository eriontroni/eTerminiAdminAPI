using eTerminiAdminAPI.Application.DTOs.Services;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;
using eTerminiAPI.Domain.Entities;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminServiceService : IAdminServiceService
{
    private readonly IUnitOfWork _uow;

    public AdminServiceService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ServiceAdminDto>> GetByInstitutionAsync(Guid institutionId, bool includeInactive = false)
    {
        var services = await _uow.PublicServices.FindAsync(s =>
            s.InstitutionId == institutionId && (includeInactive || s.IsActive));

        var insts = (await _uow.Institutions.GetAllAsync()).ToDictionary(i => i.Id);

        return services
            .OrderBy(s => s.Name)
            .Select(s => MapToDto(s, insts));
    }

    public async Task<ServiceAdminDto> GetByIdAsync(Guid id)
    {
        var service = await _uow.PublicServices.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Shërbimi me ID {id} nuk u gjet.");

        var insts = (await _uow.Institutions.GetAllAsync()).ToDictionary(i => i.Id);

        return MapToDto(service, insts);
    }

    public async Task<(bool Success, string Message, ServiceAdminDto? Service)> CreateAsync(CreateServiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i shërbimit është i detyrueshëm.", null);

        if (dto.DurationMinutes < 5 || dto.DurationMinutes > 480)
            return (false, "Kohëzgjatja duhet të jetë mes 5 dhe 480 minutave.", null);

        var institution = await _uow.Institutions.GetByIdAsync(dto.InstitutionId);
        if (institution == null)
            return (false, "Institucioni nuk u gjet.", null);

        var existing = await _uow.PublicServices.FindAsync(s =>
            s.InstitutionId == dto.InstitutionId && s.Name == dto.Name.Trim());
        if (existing.Any())
            return (false, $"Shërbimi '{dto.Name}' ekziston tashmë në këtë institucion.", null);

        var service = new PublicService
        {
            Id              = Guid.NewGuid(),
            TenantId        = institution.TenantId,
            InstitutionId   = dto.InstitutionId,
            Name            = dto.Name.Trim(),
            Description     = dto.Description?.Trim(),
            DurationMinutes = dto.DurationMinutes,
            IsActive        = true,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow,
        };

        await _uow.PublicServices.AddAsync(service);
        await _uow.SaveChangesAsync();

        return (true, "Shërbimi u krijua me sukses.", await GetByIdAsync(service.Id));
    }

    public async Task<(bool Success, string Message, ServiceAdminDto? Service)> UpdateAsync(Guid id, UpdateServiceDto dto)
    {
        var service = await _uow.PublicServices.GetByIdAsync(id);
        if (service == null)
            return (false, "Shërbimi nuk u gjet.", null);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return (false, "Emri i shërbimit është i detyrueshëm.", null);

        if (dto.DurationMinutes < 5 || dto.DurationMinutes > 480)
            return (false, "Kohëzgjatja duhet të jetë mes 5 dhe 480 minutave.", null);

        var dup = await _uow.PublicServices.FindAsync(s =>
            s.InstitutionId == service.InstitutionId && s.Name == dto.Name.Trim() && s.Id != id);
        if (dup.Any())
            return (false, $"Shërbimi '{dto.Name}' ekziston tashmë në këtë institucion.", null);

        service.Name            = dto.Name.Trim();
        service.Description     = dto.Description?.Trim();
        service.DurationMinutes = dto.DurationMinutes;
        service.UpdatedAt       = DateTime.UtcNow;

        _uow.PublicServices.Update(service);
        await _uow.SaveChangesAsync();

        return (true, "Shërbimi u përditësua me sukses.", await GetByIdAsync(id));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var service = await _uow.PublicServices.GetByIdAsync(id);
        if (service == null)
            return (false, "Shërbimi nuk u gjet.");

        var activeSlots = await _uow.TimeSlots.FindAsync(t => t.ServiceId == id && t.StartTime > DateTime.UtcNow);
        if (activeSlots.Any())
            return (false, "Ky shërbim nuk mund të fshihet sepse ka termina të ardhshme aktive.");

        _uow.PublicServices.Delete(service);
        await _uow.SaveChangesAsync();

        return (true, "Shërbimi u fshi me sukses.");
    }

    public async Task<(bool Success, string Message, ServiceAdminDto? Service)> ToggleActiveAsync(Guid id)
    {
        var service = await _uow.PublicServices.GetByIdAsync(id);
        if (service == null)
            return (false, "Shërbimi nuk u gjet.", null);

        service.IsActive  = !service.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        _uow.PublicServices.Update(service);
        await _uow.SaveChangesAsync();

        return (true, $"Shërbimi u {(service.IsActive ? "aktivizua" : "çaktivizua")} me sukses.", await GetByIdAsync(id));
    }

    private static ServiceAdminDto MapToDto(
        PublicService s,
        IReadOnlyDictionary<Guid, Institution> insts)
    {
        insts.TryGetValue(s.InstitutionId, out var inst);

        return new ServiceAdminDto
        {
            Id              = s.Id,
            Name            = s.Name,
            Description     = s.Description,
            DurationMinutes = s.DurationMinutes,
            IsActive        = s.IsActive,
            InstitutionId   = s.InstitutionId,
            InstitutionName = inst?.Name ?? string.Empty,
            CreatedAt       = s.CreatedAt,
        };
    }
}
