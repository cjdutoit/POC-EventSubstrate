// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections.Generic;
using System.Security.Claims;

namespace StudentApp.Security.Client.Models.Foundations.Users
{
    public class User
    {
        public User(
            string userId,
            string givenName,
            string surname,
            string displayName,
            string email,
            string jobTitle,
            IEnumerable<string> roles,
            IEnumerable<Claim> claims,
            string subjectId = null,
            string username = null,
            string tenantId = null,
            string clientId = null,
            string clientApplicationName = null,
            string delegatedBySubjectId = null,
            IReadOnlyList<string> scopes = null,
            IReadOnlyList<string> permissions = null,
            bool isAuthenticated = false,
            bool isSystemIdentity = false,
            string authenticationType = "Unknown")
        {
            UserId = userId;
            GivenName = givenName;
            Surname = surname;
            DisplayName = displayName;
            Email = email;
            JobTitle = jobTitle;
            Roles = roles;
            Claims = claims;
            SubjectId = subjectId;
            Username = username;
            TenantId = tenantId;
            ClientId = clientId;
            ClientApplicationName = clientApplicationName;
            DelegatedBySubjectId = delegatedBySubjectId;
            Scopes = scopes ?? new List<string>().AsReadOnly();
            Permissions = permissions ?? new List<string>().AsReadOnly();
            IsAuthenticated = isAuthenticated;
            IsSystemIdentity = isSystemIdentity;
            AuthenticationType = authenticationType;
        }

        public string UserId { get; private set; } = string.Empty;
        public string GivenName { get; private set; } = string.Empty;
        public string Surname { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string JobTitle { get; private set; } = string.Empty;
        public IEnumerable<string> Roles { get; private set; }
        public IEnumerable<Claim> Claims { get; private set; }
        public string SubjectId { get; private set; }
        public string Username { get; private set; }
        public string TenantId { get; private set; }
        public string ClientId { get; private set; }
        public string ClientApplicationName { get; private set; }
        public string DelegatedBySubjectId { get; private set; }
        public IReadOnlyList<string> Scopes { get; private set; }
        public IReadOnlyList<string> Permissions { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public bool IsSystemIdentity { get; private set; }
        public string AuthenticationType { get; private set; }
    }
}
