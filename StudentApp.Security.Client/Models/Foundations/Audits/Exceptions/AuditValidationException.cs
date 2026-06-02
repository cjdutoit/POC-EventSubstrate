// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Audits.Exceptions
{
    internal class AuditValidationException : Xeption
    {
        public AuditValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}