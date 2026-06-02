// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Orchestrations.Students.Exceptions
{
    public class InvalidStudentOrchestrationException : Xeption
    {
        public InvalidStudentOrchestrationException(string message)
            : base(message) { }
    }
}
