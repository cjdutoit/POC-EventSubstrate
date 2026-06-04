// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        public async ValueTask<Enrollment> InsertEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(enrollment, cancellationToken);

        public IQueryable<Enrollment> SelectAllEnrollments() =>
            SelectAll<Enrollment>();

        public async ValueTask<Enrollment> SelectEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default) =>
            await SelectByIdAsync<Enrollment>(new object[] { enrollmentId }, cancellationToken);

        public async ValueTask<Enrollment> UpdateEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync(enrollment, cancellationToken);

        public async ValueTask<Enrollment> DeleteEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(enrollment, cancellationToken);
    }
}
