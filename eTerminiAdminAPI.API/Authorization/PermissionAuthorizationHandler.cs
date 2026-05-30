using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace eTerminiAdminAPI.API.Authorization;

/// <summary>
/// Lejon qasje nëse përdoruesi është SuperAdmin (qasje e plotë) ose ka claim-in
/// "permission" që përputhet me lejen e kërkuar.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public const string PermissionClaimType = "permission";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // SuperAdmin → qasje e plotë, pa nevojë për leje specifike.
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims.Any(c =>
            c.Type == PermissionClaimType && c.Value == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
