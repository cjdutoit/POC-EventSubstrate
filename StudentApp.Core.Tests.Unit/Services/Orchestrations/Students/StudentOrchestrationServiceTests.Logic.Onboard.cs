// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldOnboardStudentAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student returnedStudent = randomStudent;
            Student expectedStudent = returnedStudent;
            Enrollment returnedEnrollment = CreateRandomEnrollment(inputStudent.Id);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedStudent);

            this.enrollmentServiceMock.Setup(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e =>
                        e.StudentId == inputStudent.Id &&
                        e.CourseCode == "DEFAULT-101"),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(returnedEnrollment);

            // when
            Student actualStudent =
                await this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            // then
            actualStudent.Should().BeEquivalentTo(expectedStudent);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.enrollmentServiceMock.Verify(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e =>
                        e.StudentId == inputStudent.Id &&
                        e.CourseCode == "DEFAULT-101"),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Student {actualStudent.Id} onboarded — events dispatched via substrate."),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
