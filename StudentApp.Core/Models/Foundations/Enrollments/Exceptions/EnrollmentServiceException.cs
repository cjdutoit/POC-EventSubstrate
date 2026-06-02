// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class EnrollmentServiceException : Xeption
    {
        public EnrollmentServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
