// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Orchestrations.Audits.Exceptions
{
    internal class AuditOrchestrationValidationException : Xeption
    {
        public AuditOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
