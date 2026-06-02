// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using Xeptions;

namespace StudentApp.Security.Client.Models.Orchestrations.Audits.Exceptions
{
    internal class FailedAuditOrchestrationServiceException : Xeption
    {
        public FailedAuditOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}