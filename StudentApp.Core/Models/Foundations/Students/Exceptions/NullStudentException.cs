// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Students.Exceptions
{
    public class NullStudentException : Xeption
    {
        public NullStudentException(string message)
            : base(message)
        {
        }
    }
}
