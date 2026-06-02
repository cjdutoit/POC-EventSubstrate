// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Students.Exceptions
{
    public class StudentServiceException : Xeption
    {
        public StudentServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
