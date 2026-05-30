using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace eTerminiAdminAPI.API.Authorization;

/// <summary>
/// Ndërton politika autorizimi on-the-fly për emra që fillojnë me "perm:".
/// Kjo shmang nevojën për të regjistruar manualisht çdo leje si politikë.
/// </summary>
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(HasPermissionAttribute.PolicyPrefix.Length);
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
