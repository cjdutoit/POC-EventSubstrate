// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        public async ValueTask<Student> InsertStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(student, cancellationToken);

        public IQueryable<Student> SelectAllStudents() =>
            SelectAll<Student>();

        public async ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
            await SelectByIdAsync<Student>(new object[] { studentId }, cancellationToken);

        public async ValueTask<Student> UpdateStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            await UpdateAsync(student, cancellationToken);

        public async ValueTask<Student> DeleteStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            await DeleteAsync(student, cancellationToken);
    }
}
