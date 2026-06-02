// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Services.Foundations.Students
{
    public interface IStudentService : IEventReceiver<StudentEnrolledEvent>
    {
        ValueTask<Student> AddStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);

        IQueryable<Student> RetrieveAllStudents();

        ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);

        ValueTask<Student> ModifyStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);

        ValueTask<Student> RemoveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }
}
