// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Timetables.Exceptions
{
    public class InvalidTimetableException : Xeption
    {
        public InvalidTimetableException(string message)
            : base(message)
        {
        }
    }
}
