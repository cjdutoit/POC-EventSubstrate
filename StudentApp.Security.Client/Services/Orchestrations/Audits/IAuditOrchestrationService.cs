// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Security.Claims;
using System.Threading.Tasks;
using StudentApp.Security.Client.Models.Clients;

namespace StudentApp.Security.Client.Services.Orchestrations.Audits
{
    internal interface IAuditOrchestrationService
    {
        ValueTask<T> ApplyAddAuditValuesAsync<T>(
            T entity,
            ClaimsPrincipal claimsPrincipal,
            SecurityConfigurations securityConfigurations);

        ValueTask<T> ApplyModifyAuditValuesAsync<T>(
            T entity,
            ClaimsPrincipal claimsPrincipal,
            SecurityConfigurations securityConfigurations);

        ValueTask<T> ApplyRemoveAuditValuesAsync<T>(
            T entity,
            ClaimsPrincipal claimsPrincipal,
            SecurityConfigurations securityConfigurations);

        ValueTask<T> EnsureAddAuditValuesRemainsUnchangedOnModifyAsync<T>(
            T entity,
            T storageEntity,
            SecurityConfigurations securityConfigurations);

        ValueTask<string> GetCurrentUserIdAsync(ClaimsPrincipal claimsPrincipal);
    }
}
