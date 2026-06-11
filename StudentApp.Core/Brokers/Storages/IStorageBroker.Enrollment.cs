// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Brokers.Storages
{
    public partial interface IStorageBroker
    {
        ValueTask<Enrollment> InsertEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default);

        IQueryable<Enrollment> SelectAllEnrollments();

        ValueTask<Enrollment> SelectEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default);

        ValueTask<Enrollment> UpdateEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default);

        ValueTask<Enrollment> DeleteEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default);
    }
}
