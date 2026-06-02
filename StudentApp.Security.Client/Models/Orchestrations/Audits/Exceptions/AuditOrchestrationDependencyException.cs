// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Orchestrations.Audits.Exceptions
{
    internal class AuditOrchestrationDependencyException : Xeption
    {
        public AuditOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
