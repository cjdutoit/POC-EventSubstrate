// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Orchestrations.Students.Exceptions
{
    public class StudentOrchestrationServiceException : Xeption
    {
        public StudentOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException) { }
    }
}
