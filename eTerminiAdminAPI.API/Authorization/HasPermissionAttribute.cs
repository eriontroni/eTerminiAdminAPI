using Microsoft.AspNetCore.Authorization;

namespace eTerminiAdminAPI.API.Authorization;

/// <summary>
/// Kërkon një leje specifike për endpoint-in. Prefiksi "perm:" përdoret nga
/// PermissionPolicyProvider për të ndërtuar politikën dinamikisht.
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public HasPermissionAttribute(string permission) => Policy = $"{PolicyPrefix}{permission}";
}
