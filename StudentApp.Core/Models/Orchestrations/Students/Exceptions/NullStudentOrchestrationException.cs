// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Orchestrations.Students.Exceptions
{
    public class NullStudentOrchestrationException : Xeption
    {
        public NullStudentOrchestrationException(string message)
            : base(message) { }
    }
}
