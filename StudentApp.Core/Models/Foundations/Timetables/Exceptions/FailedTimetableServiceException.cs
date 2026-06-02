// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Core.Models.Foundations.Timetables.Exceptions
{
    public class FailedTimetableServiceException : Xeption
    {
        public FailedTimetableServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        {
        }
    }
}
