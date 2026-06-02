// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Orchestrations.Students.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnOnboardIfStudentIsNullAsync()
        {
            // given
            Student nullStudent = null;

            var nullStudentOrchestrationException =
                new NullStudentOrchestrationException(message: "Student is null.");

            var expectedStudentOrchestrationValidationException =
                new StudentOrchestrationValidationException(
                    message: "Student orchestration validation error occurred, fix the errors and try again.",
                    innerException: nullStudentOrchestrationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(nullStudent);

            StudentOrchestrationValidationException actualStudentOrchestrationValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationValidationException))),
                        Times.Once);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(
                    It.IsAny<Student>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnOnboardIfStudentIsInvalidAsync(
            string invalidText)
        {
            // given
            var invalidStudent = new Student
            {
                Id = Guid.Empty,
                FirstName = invalidText,
                LastName = invalidText,
                Email = invalidText
            };

            var invalidStudentOrchestrationException =
                new InvalidStudentOrchestrationException(
                    message: "Invalid student. Please correct the errors and try again.");

            invalidStudentOrchestrationException.UpsertDataList(
                key: nameof(Student.Id),
                value: "Id is required");

            invalidStudentOrchestrationException.UpsertDataList(
                key: nameof(Student.FirstName),
                value: "Text is required");

            invalidStudentOrchestrationException.UpsertDataList(
                key: nameof(Student.LastName),
                value: "Text is required");

            invalidStudentOrchestrationException.UpsertDataList(
                key: nameof(Student.Email),
                value: "Text is required");

            var expectedStudentOrchestrationValidationException =
                new StudentOrchestrationValidationException(
                    message: "Student orchestration validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentOrchestrationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(invalidStudent);

            StudentOrchestrationValidationException actualStudentOrchestrationValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationValidationException))),
                        Times.Once);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(
                    It.IsAny<Student>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
