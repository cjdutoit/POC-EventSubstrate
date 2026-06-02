// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using Xeptions;

namespace StudentApp.Security.Client.Models.Orchestrations.Audits.Exceptions
{
    internal class AuditOrchestrationServiceException : Xeption
    {
        public AuditOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}