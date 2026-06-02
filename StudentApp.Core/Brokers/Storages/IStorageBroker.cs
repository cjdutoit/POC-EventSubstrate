// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public interface IStorageBroker
    {
        ValueTask<Student> InsertStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);

        IQueryable<Student> SelectAllStudents();

        ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);

        ValueTask<Student> UpdateStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);

        ValueTask<Student> DeleteStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);

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
