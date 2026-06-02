// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Foundations.Enrollments
{
    public sealed class Enrollment
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public DateTimeOffset EnrolledAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
