// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using StudentApp.Security.Client.Models.Foundations.Users;

namespace StudentApp.Security.Client.Services.Foundations.Users
{
    internal interface IUserService
    {
        ValueTask<User> GetUserAsync(ClaimsPrincipal claimsPrincipal);
        ValueTask<string> GetUserIdAsync(ClaimsPrincipal claimsPrincipal);
        ValueTask<bool> IsUserAuthenticatedAsync(ClaimsPrincipal claimsPrincipal);
        ValueTask<bool> IsUserInRoleAsync(ClaimsPrincipal claimsPrincipal, string roleName);
        ValueTask<bool> UserHasClaimAsync(ClaimsPrincipal claimsPrincipal, string claimType, string claimValue);
        ValueTask<bool> UserHasClaimAsync(ClaimsPrincipal claimsPrincipal, string claimType);
        ValueTask<string> GetUserClaimValueAsync(ClaimsPrincipal claimsPrincipal, string type);
        ValueTask<IReadOnlyList<string>> GetUserClaimValuesAsync(ClaimsPrincipal claimsPrincipal, string type);
    }
}
