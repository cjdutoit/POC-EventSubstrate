// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.EnrollmentEvents
{
    public sealed class EnrollmentAddedEvent
    {
        public Guid EnrollmentId { get; init; }
        public Guid StudentId { get; init; }
        public string CourseCode { get; init; }
        public DateTimeOffset EnrolledAt { get; init; }
        public string Status { get; init; }
    }
}
