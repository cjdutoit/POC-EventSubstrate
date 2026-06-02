// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Audits.Exceptions
{
    internal class FailedAuditServiceException : Xeption
    {
        public FailedAuditServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}