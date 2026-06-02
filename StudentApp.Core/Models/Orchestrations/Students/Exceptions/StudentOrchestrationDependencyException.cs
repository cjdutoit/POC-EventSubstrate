// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Orchestrations.Students.Exceptions
{
    public class StudentOrchestrationDependencyException : Xeption
    {
        public StudentOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException) { }
    }
}
