// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StudentApp.Core.Models.Events;
using StudentApp.Security.Client.Clients;
using StudentApp.Security.Client.Models.Clients;

namespace StudentApp.Core.Brokers.Securities
{
    /// <summary>
    /// Provides security-related functionalities such as user authentication, claim verification, and role checks.
    /// Supports both REST API (using <see cref="IHttpContextAccessor"/>) and Azure Functions (using access token).
    /// </summary>
    public class SecurityBroker : ISecurityBroker
    {
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly ISecurityClient securityClient;
        private readonly SecurityConfigurations securityConfigurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class 
        /// using <see cref="IHttpContextAccessor"/>.
        /// This constructor is intended for REST API usage.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public SecurityBroker(IHttpContextAccessor httpContextAccessor, SecurityConfigurations securityConfigurations)
        {
            claimsPrincipal = httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
            this.securityClient = new SecurityClient();
            this.securityConfigurations = securityConfigurations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class using an access token.
        /// This constructor is intended for Azure Function / non REST API usage.
        /// </summary>
        /// <param name="accessToken">A JWT access token containing user claims.</param>
        public SecurityBroker(string accessToken, SecurityConfigurations securityConfigurations)
        {
            claimsPrincipal = GetClaimsPrincipalFromToken(accessToken);
            this.securityClient = new SecurityClient();
            this.securityConfigurations = securityConfigurations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBroker"/> class using a <see cref="ClaimsPrincipal"/>.
        /// This constructor is intended for Azure Functions or non-REST API usage.
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> containing user claims.</param>
        public SecurityBroker(ClaimsPrincipal claimsPrincipal, SecurityConfigurations securityConfigurations)
        {
            this.claimsPrincipal = claimsPrincipal;
            this.securityConfigurations = securityConfigurations;
            this.securityClient = new SecurityClient();
        }

        /// <summary>
        /// Retrieves the user identifier from the given claims principal.
        /// </summary>
        /// <param name="claimsPrincipal">The user context containing claims.</param>
        /// <returns>The user identifier string.</returns>
        /// <remarks>
        /// If no valid user identifier is found, a fallback (such as <c>"Anonymous"</c>) may be returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// string userId = await auditClient.GetUserIdAsync(User);
        /// // e.g. "Alice" or "Anonymous"
        /// </code>
        /// </example>
        public async ValueTask<string> GetUserIdAsync() =>
            await securityClient.Audits.GetUserIdAsync(claimsPrincipal);

        /// <summary>
        /// Retrieves the <see cref="SecurityContext"/> for the current authenticated caller.
        /// </summary>
        /// <returns>A <see cref="SecurityContext"/> mapped from the current <see cref="ClaimsPrincipal"/>.</returns>
        public async ValueTask<SecurityContext> GetCurrentSecurityContextAsync()
        {
            var user = await this.securityClient.Users.GetUserAsync(claimsPrincipal);

            return new SecurityContext
            {
                SubjectId = user.SubjectId,
                Username = user.Username,
                TenantId = user.TenantId,
                ClientId = user.ClientId,
                ClientApplicationName = user.ClientApplicationName,
                DelegatedBySubjectId = user.DelegatedBySubjectId,
                Roles = (IReadOnlyList<string>)user.Roles ?? new List<string>().AsReadOnly(),
                Scopes = user.Scopes,
                Permissions = user.Permissions,
                IsAuthenticated = user.IsAuthenticated,
                IsSystemIdentity = user.IsSystemIdentity,
                AuthenticationType = Enum.Parse<AuthenticationType>(user.AuthenticationType)
            };
        }

        /// <summary>
        /// Determines whether the current user is authenticated.
        /// </summary>
        /// <returns>True if the user is authenticated; otherwise, false.</returns>
        public async ValueTask<bool> IsCurrentUserAuthenticatedAsync() =>
            await this.securityClient.Users.IsUserAuthenticatedAsync(claimsPrincipal);

        /// <summary>
        /// Checks if the current user is in a specified role.
        /// </summary>
        /// <param name="roleName">The role name to check.</param>
        /// <returns>True if the user is in the specified role; otherwise, false.</returns>
        public async ValueTask<bool> IsInRoleAsync(string roleName) =>
            await this.securityClient.Users.IsUserInRoleAsync(claimsPrincipal, roleName);

        /// <summary>
        /// Checks whether the current user has a specific claim with a given value.
        /// </summary>
        /// <param name="claimType">The type of the claim.</param>
        /// <param name="claimValue">The value of the claim.</param>
        /// <returns>True if the user has the claim with the specified value; otherwise, false.</returns>
        public async ValueTask<bool> UserHasClaimAsync(string claimType, string claimValue) =>
            await this.securityClient.Users.UserHasClaimAsync(claimsPrincipal, claimType, claimValue);

        /// <summary>
        /// Checks whether the current user has a specific claim type.
        /// </summary>
        /// <param name="claimType">The type of the claim.</param>
        /// <returns>True if the user has the claim; otherwise, false.</returns>
        public async ValueTask<bool> UserHasClaimAsync(string claimType) =>
            await this.securityClient.Users.UserHasClaimAsync(claimsPrincipal, claimType);

        /// <summary>
        /// Applies auditing metadata for an add operation to the specified entity.
        /// Sets created and updated audit fields based on the current user.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit.</param>
        /// <returns>The audited entity with add metadata applied.</returns>
        public ValueTask<T> ApplyAddAuditValuesAsync<T>(T entity) =>
            this.securityClient.Audits.ApplyAddAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a modify operation to the specified entity.
        /// Sets updated audit fields based on the current user.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit.</param>
        /// <returns>The audited entity with modify metadata applied.</returns>
        public ValueTask<T> ApplyModifyAuditValuesAsync<T>(T entity) =>
                this.securityClient.Audits.ApplyModifyAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Applies auditing metadata for a remove (soft delete) operation to the specified entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity to audit for removal.</param>
        /// <returns>The audited entity with remove metadata applied.</returns>
        public ValueTask<T> ApplyRemoveAuditValuesAsync<T>(T entity) =>
                this.securityClient.Audits.ApplyRemoveAuditValuesAsync(entity, claimsPrincipal, securityConfigurations);

        /// <summary>
        /// Ensures that add audit values (e.g., created by/date) remain unchanged during modify operations.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity being modified.</param>
        /// <param name="storageEntity">The original stored entity used to preserve original audit values.</param>
        /// <returns>The entity with original add audit values retained.</returns>
        public ValueTask<T> EnsureAddAuditValuesRemainsUnchangedOnModifyAsync<T>(
            T entity,
            T storageEntity) =>
                this.securityClient.Audits
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(entity, storageEntity, securityConfigurations);

        /// <summary>
        /// Extracts a <see cref="ClaimsPrincipal"/> from a given JWT token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> containing claims from the token.</returns>
        private static ClaimsPrincipal GetClaimsPrincipalFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

            return new ClaimsPrincipal(identity);
        }
    }
}
