// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using StudentApp.Security.Client.Models.Foundations.Users;
using StudentApp.Security.Client.Models.Foundations.Users.Exceptions;

namespace StudentApp.Security.Client.Services.Foundations.Users
{
    internal partial class UserService : IUserService
    {
        public ValueTask<User> GetUserAsync(ClaimsPrincipal claimsPrincipal) =>
        TryCatch(async () =>
        {
            ValidateOnGetUser(claimsPrincipal);

            return GetUserFromClaimsPrincipal(claimsPrincipal);
        });

        public ValueTask<string> GetUserIdAsync(ClaimsPrincipal claimsPrincipal) =>
        TryCatch(async () =>
        {
            ValidateOnGetUserId(claimsPrincipal);
            var user = GetUserFromClaimsPrincipal(claimsPrincipal);
            var isAuthenticated = claimsPrincipal.Identity?.IsAuthenticated ?? false;

            string userId = isAuthenticated
                ? user.UserId
                : string.IsNullOrEmpty(user.UserId)
                    ? "anonymous" : user.UserId;

            return userId;
        });

        public ValueTask<bool> UserHasClaimAsync(
            ClaimsPrincipal claimsPrincipal,
            string claimType,
            string claimValue) =>
        TryCatch(async () =>
        {
            ValidateOnUserHasClaimType(claimsPrincipal, claimType, claimValue);

            return claimsPrincipal.HasClaim(claimType, claimValue);
        });

        public ValueTask<bool> UserHasClaimAsync(ClaimsPrincipal claimsPrincipal, string claimType) =>
        TryCatch(async () =>
        {
            ValidateOnUserHasClaimType(claimsPrincipal, claimType);

            return claimsPrincipal.FindFirst(claimType) != null;
        });

        public ValueTask<bool> IsUserAuthenticatedAsync(ClaimsPrincipal claimsPrincipal) =>
        TryCatch(async () =>
        {
            ValidateOnIsUserAuthenticated(claimsPrincipal);

            return claimsPrincipal.Identity?.IsAuthenticated ?? false;
        });

        public ValueTask<bool> IsUserInRoleAsync(ClaimsPrincipal claimsPrincipal, string roleName) =>
        TryCatch(async () =>
        {
            ValidateOnIsUserInRole(claimsPrincipal, roleName);
            var roles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(role => role.Value);

            return roles.Contains(roleName);
        });

        public ValueTask<string> GetUserClaimValueAsync(ClaimsPrincipal claimsPrincipal, string type) =>
        TryCatch(async () =>
        {
            ValidateOnGetUserClaimValue(claimsPrincipal, type);

            var claim = claimsPrincipal.FindFirst(type);

            if (claim is null)
            {
                throw new ClaimNotFoundUserException($"Claim with type '{type}' not found.");
            }

            return claim.Value;
        });

        public ValueTask<IReadOnlyList<string>> GetUserClaimValuesAsync(ClaimsPrincipal claimsPrincipal, string type) =>
        TryCatch<IReadOnlyList<string>>(async () =>
        {
            ValidateOnGetUserClaimValue(claimsPrincipal, type);

            var values = claimsPrincipal.FindAll(type)
                .Select(c => c.Value)
                .ToArray();

            if (values.Count() == 0)
            {
                throw new ClaimNotFoundUserException($"Claim with type '{type}' not found.");
            }

            return values;
        });

        private static User GetUserFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            var subjectId =
                claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value
                ?? claimsPrincipal.FindFirst("oid")?.Value
                ?? claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? claimsPrincipal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var username =
                claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                ?? claimsPrincipal.FindFirst("preferred_username")?.Value
                ?? claimsPrincipal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            var tenantId =
                claimsPrincipal.FindFirst("tid")?.Value
                ?? claimsPrincipal.FindFirst("tenant_id")?.Value;

            var clientId =
                claimsPrincipal.FindFirst("azp")?.Value
                ?? claimsPrincipal.FindFirst("client_id")?.Value;

            var delegatedBySubjectId =
                claimsPrincipal.FindFirst("act")?.Value
                ?? claimsPrincipal.FindFirst("on_behalf_of")?.Value;

            var givenName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
            var surname = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
            var displayName = claimsPrincipal.FindFirst("displayName")?.Value;
            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var jobTitle = claimsPrincipal.FindFirst("jobTitle")?.Value;
            var roles = claimsPrincipal.FindAll(ClaimTypes.Role).Select(role => role.Value).ToList();
            var claimsList = claimsPrincipal.Claims;

            var scopes = (claimsPrincipal.FindFirst("scp")?.Value
                ?? claimsPrincipal.FindFirst("scope")?.Value ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .AsReadOnly();

            var permissions = claimsPrincipal
                .FindAll("permission")
                .Select(c => c.Value)
                .ToList()
                .AsReadOnly();

            bool isAuthenticated = claimsPrincipal.Identity?.IsAuthenticated ?? false;
            bool isSystemIdentity = string.IsNullOrWhiteSpace(subjectId) && !string.IsNullOrWhiteSpace(clientId);

            string authenticationType = isSystemIdentity
                ? "Machine"
                : !string.IsNullOrWhiteSpace(delegatedBySubjectId)
                    ? "Delegated"
                    : isAuthenticated
                        ? "User"
                        : "Unknown";

            return new User(
                userId: subjectId,
                givenName: givenName,
                surname: surname,
                displayName: displayName,
                email: email,
                jobTitle: jobTitle,
                roles: roles,
                claims: claimsList,
                subjectId: subjectId,
                username: username,
                tenantId: tenantId,
                clientId: clientId,
                clientApplicationName: claimsPrincipal.FindFirst("app_displayname")?.Value,
                delegatedBySubjectId: delegatedBySubjectId,
                scopes: scopes,
                permissions: permissions,
                isAuthenticated: isAuthenticated,
                isSystemIdentity: isSystemIdentity,
                authenticationType: authenticationType);
        }
    }
}
