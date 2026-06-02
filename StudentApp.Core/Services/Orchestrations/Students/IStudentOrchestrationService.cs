// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Services.Orchestrations.Students
{
    public interface IStudentOrchestrationService
    {
        ValueTask<Student> OnboardStudentAsync(
            Student student,
            CancellationToken cancellationToken = default);
    }
}
