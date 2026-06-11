// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Glory2Him.Core.Models.Bases;

namespace StudentApp.Core.Models.Foundations.Enrollments
{
    public sealed class Enrollment : IKey, IAudit
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public DateTimeOffset EnrolledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedWhen { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset UpdatedWhen { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset? DeletedWhen { get; set; }
        public string? DeletionReason { get; set; }
    }
}
