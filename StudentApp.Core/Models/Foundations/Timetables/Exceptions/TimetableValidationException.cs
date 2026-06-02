// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Timetables.Exceptions
{
    public class TimetableValidationException : Xeption
    {
        public TimetableValidationException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
