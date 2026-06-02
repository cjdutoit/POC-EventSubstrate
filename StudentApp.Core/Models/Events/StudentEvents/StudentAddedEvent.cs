// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.StudentEvents
{
    public sealed class StudentAddedEvent
    {
        public Guid StudentId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public string Email { get; init; }
        public string Status { get; init; }
    }
}
