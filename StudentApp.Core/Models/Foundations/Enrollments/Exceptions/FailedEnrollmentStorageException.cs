// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Core.Models.Foundations.Enrollments.Exceptions
{
    public class FailedEnrollmentStorageException : Xeption
    {
        public FailedEnrollmentStorageException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        {
        }
    }
}
