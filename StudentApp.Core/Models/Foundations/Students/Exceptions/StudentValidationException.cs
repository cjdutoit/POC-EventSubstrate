// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Students.Exceptions
{
    public class StudentValidationException : Xeption
    {
        public StudentValidationException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
