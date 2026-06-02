// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.StudentEvents
{
    public sealed class StudentEnrolledEvent
    {
        public Guid StudentId { get; init; }
        public string Status { get; init; }
    }
}
