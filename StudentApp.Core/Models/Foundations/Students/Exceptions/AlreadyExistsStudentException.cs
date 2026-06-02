// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Core.Models.Foundations.Students.Exceptions
{
    public class AlreadyExistsStudentException : Xeption
    {
        public AlreadyExistsStudentException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        {
        }
    }
}
