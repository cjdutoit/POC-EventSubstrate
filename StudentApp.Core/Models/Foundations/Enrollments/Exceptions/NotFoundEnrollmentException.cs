// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class NotFoundEnrollmentException : Xeption
    {
        public NotFoundEnrollmentException(string message)
            : base(message)
        {
        }
    }
}
