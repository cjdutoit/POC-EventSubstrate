// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Audits.Exceptions
{
    internal class InvalidArgumentAuditException : Xeption
    {
        public InvalidArgumentAuditException(string message)
            : base(message)
        { }
    }
}