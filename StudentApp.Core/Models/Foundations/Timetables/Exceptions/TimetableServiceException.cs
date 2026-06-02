// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Timetables.Exceptions
{
    public class TimetableServiceException : Xeption
    {
        public TimetableServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
