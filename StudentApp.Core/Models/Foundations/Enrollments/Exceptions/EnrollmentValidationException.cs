// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class EnrollmentValidationException : Xeption
    {
        public EnrollmentValidationException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
