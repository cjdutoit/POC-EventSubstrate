// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Services.Foundations.Enrollments
{
    public interface IEnrollmentService
    {
        ValueTask<Enrollment> AddEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default);

        IQueryable<Enrollment> RetrieveAllEnrollments();

        ValueTask<Enrollment> RetrieveEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default);

        ValueTask<Enrollment> ModifyEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default);

        ValueTask<Enrollment> RemoveEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default);
    }
}
