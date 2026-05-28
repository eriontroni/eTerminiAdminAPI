using eTerminiAdminAPI.Application.DTOs.System;
using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAPI.Application.Interfaces.Repositories;

namespace eTerminiAdminAPI.Infrastructure.Services;

public class AdminSystemService : IAdminSystemService
{
    private readonly IUnitOfWork _uow;

    public AdminSystemService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(int page = 1, int pageSize = 50)
    {
        var logs = await _uow.AuditLogs.GetAllAsync();
        return logs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogDto
            {
                Id         = l.Id,
                UserId     = l.UserId,
                Action     = l.Action,
                EntityName = l.EntityName,
                EntityId   = l.EntityId,
                OldValues  = l.OldValues,
                NewValues  = l.NewValues,
                IpAddress  = l.IpAddress,
                CreatedAt  = l.CreatedAt
            });
    }

    public async Task<IEnumerable<UserAdminDto>> GetUsersAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return users.Select(u => new UserAdminDto
        {
            Id        = u.Id,
            FullName  = $"{u.FirstName} {u.LastName}",
            Email     = u.Email,
            Role      = u.Role.ToString(),
            IsActive  = u.IsActive,
            CreatedAt = u.CreatedAt
        }).OrderBy(u => u.FullName);
    }
}
