using eTerminiAdminAPI.Application.Interfaces.Services;
using eTerminiAdminAPI.Infrastructure.Services;
using eTerminiAPI.Application.Interfaces.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eTerminiAdminAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        eTerminiAPI.Infrastructure.DependencyInjection.AddInfrastructure(services, configuration);

        // AdminAPI nuk ka SignalR — regjistro stub no-op për varësinë e AppointmentService
        services.AddScoped<ISlotAvailabilityBroadcaster, NoOpSlotBroadcaster>();

        services.AddScoped<IAdminAuthService,        AdminAuthService>();
        services.AddScoped<IAdminInstitutionService, AdminInstitutionService>();
        services.AddScoped<IAdminWorkerService,      AdminWorkerService>();
        services.AddScoped<IAdminDashboardService,   AdminDashboardService>();
        services.AddScoped<IAdminSystemService,      AdminSystemService>();
        services.AddScoped<IAdminTenantService,      AdminTenantService>();
        services.AddScoped<IAdminDepartmentService,  AdminDepartmentService>();
        services.AddScoped<IAdminCategoryService,    AdminCategoryService>();
        services.AddScoped<IAdminRoleService,          AdminRoleService>();
        services.AddScoped<IAdminAdministratorService, AdminAdministratorService>();

        return services;
    }
}

file sealed class NoOpSlotBroadcaster : ISlotAvailabilityBroadcaster
{
    public Task SlotsChangedAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
