// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Orchestrations.Audits.Exceptions
{
    internal class InvalidArgumentAuditOrchestrationException : Xeption
    {
        public InvalidArgumentAuditOrchestrationException(string message)
            : base(message)
        { }
    }
}