// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Services.Foundations.Enrollments;
using StudentApp.Core.Services.Foundations.Students;

namespace StudentApp.Core.Services.Orchestrations.Students
{
    public sealed partial class StudentOrchestrationService : IStudentOrchestrationService
    {
        private readonly IStudentService studentService;
        private readonly IEnrollmentService enrollmentService;
        private readonly ILoggingBroker loggingBroker;

        public StudentOrchestrationService(
            IStudentService studentService,
            IEnrollmentService enrollmentService,
            ILoggingBroker loggingBroker)
        {
            this.studentService = studentService;
            this.enrollmentService = enrollmentService;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Student> OnboardStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateStudentOnOnboard(student);

                this.loggingBroker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {student.Id}");

                Student addedStudent =
                    await this.studentService.AddStudentAsync(
                        student,
                        cancellationToken);

                var enrollment = new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentId = addedStudent.Id,
                    CourseCode = "DEFAULT-101"
                };

                await this.enrollmentService.AddEnrollmentAsync(
                    enrollment,
                    cancellationToken);

                this.loggingBroker.LogInformation(
                    $"[StudentOrchestrationService] Student {addedStudent.Id} onboarded — events dispatched via substrate.");

                return addedStudent;
            });
    }
}
