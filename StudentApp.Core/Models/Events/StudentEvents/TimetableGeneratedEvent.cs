// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.StudentEvents
{
    public sealed class TimetableGeneratedEvent
    {
        public Guid StudentId { get; init; }
    }
}
