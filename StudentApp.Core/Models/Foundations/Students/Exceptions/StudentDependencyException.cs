// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Students.Exceptions
{
    public class StudentDependencyException : Xeption
    {
        public StudentDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
