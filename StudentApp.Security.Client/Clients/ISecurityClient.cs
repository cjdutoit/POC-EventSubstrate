// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Security.Client.Clients.Audits;
using StudentApp.Security.Client.Clients.Users;

namespace StudentApp.Security.Client.Clients
{
    public interface ISecurityClient
    {
        IUserClient Users { get; }
        IAuditClient Audits { get; }
    }
}
