// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class EnrollmentDependencyException : Xeption
    {
        public EnrollmentDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
