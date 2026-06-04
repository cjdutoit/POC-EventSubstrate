// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public partial interface IStorageBroker
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
    }
}
