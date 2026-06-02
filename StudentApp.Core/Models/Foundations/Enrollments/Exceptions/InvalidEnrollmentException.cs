// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class InvalidEnrollmentException : Xeption
    {
        public InvalidEnrollmentException(string message)
            : base(message)
        {
        }
    }
}
