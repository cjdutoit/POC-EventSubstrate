// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class NullEnrollmentException : Xeption
    {
        public NullEnrollmentException(string message)
            : base(message)
        {
        }
    }
}
