// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Foundations.Students.Exceptions;
using StudentApp.Core.Models.Orchestrations.Students.Exceptions;
using Xeptions;

namespace StudentApp.Core.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowOrchestrationDependencyValidationExceptionOnOnboardIfStudentValidationExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;

            var invalidStudentException =
                new InvalidStudentException(
                    message: "Invalid student. Please correct the errors and try again.");

            var studentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: invalidStudentException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: studentValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentValidationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyValidationException actualStudentOrchestrationDependencyValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyValidationException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationDependencyValidationExceptionOnOnboardIfStudentDependencyValidationExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;

            var alreadyExistsStudentException =
                new AlreadyExistsStudentException(
                    message: "Student with the same id already exists.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var studentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsStudentException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: studentDependencyValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentDependencyValidationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyValidationException actualStudentOrchestrationDependencyValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyValidationException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationDependencyExceptionOnOnboardIfStudentDependencyExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var studentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);

            var expectedStudentOrchestrationDependencyException =
                new StudentOrchestrationDependencyException(
                    message: "Student orchestration dependency error occurred, contact support.",
                    innerException: studentDependencyException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentDependencyException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyException actualStudentOrchestrationDependencyException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationServiceExceptionOnOnboardIfStudentServiceExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;

            var failedStudentServiceException =
                new FailedStudentServiceException(
                    message: "Failed student service error occurred, contact support.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var studentServiceException =
                new StudentServiceException(
                    message: "Student service error occurred, contact support.",
                    innerException: failedStudentServiceException);

            var expectedStudentOrchestrationServiceException =
                new StudentOrchestrationServiceException(
                    message: "Student orchestration service error occurred, contact support.",
                    innerException: studentServiceException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(studentServiceException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationServiceException actualStudentOrchestrationServiceException =
                await Assert.ThrowsAsync<StudentOrchestrationServiceException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationServiceException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationServiceException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationDependencyValidationExceptionOnOnboardIfEnrollmentValidationExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student returnedStudent = randomStudent;

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            var enrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: enrollmentValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedStudent);

            this.enrollmentServiceMock.Setup(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(enrollmentValidationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyValidationException actualStudentOrchestrationDependencyValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.enrollmentServiceMock.Verify(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyValidationException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationDependencyValidationExceptionOnOnboardIfEnrollmentDependencyValidationExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student returnedStudent = randomStudent;

            var alreadyExistsEnrollmentException =
                new AlreadyExistsEnrollmentException(
                    message: "Enrollment with the same id already exists.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var enrollmentDependencyValidationException =
                new EnrollmentDependencyValidationException(
                    message: "Enrollment dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsEnrollmentException);

            var expectedStudentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: enrollmentDependencyValidationException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedStudent);

            this.enrollmentServiceMock.Setup(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(enrollmentDependencyValidationException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyValidationException actualStudentOrchestrationDependencyValidationException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyValidationException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyValidationException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.enrollmentServiceMock.Verify(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyValidationException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationDependencyExceptionOnOnboardIfEnrollmentDependencyExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student returnedStudent = randomStudent;

            var failedEnrollmentStorageException =
                new FailedEnrollmentStorageException(
                    message: "Failed enrollment storage error occurred, contact support.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var enrollmentDependencyException =
                new EnrollmentDependencyException(
                    message: "Enrollment dependency error occurred, contact support.",
                    innerException: failedEnrollmentStorageException);

            var expectedStudentOrchestrationDependencyException =
                new StudentOrchestrationDependencyException(
                    message: "Student orchestration dependency error occurred, contact support.",
                    innerException: enrollmentDependencyException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedStudent);

            this.enrollmentServiceMock.Setup(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(enrollmentDependencyException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationDependencyException actualStudentOrchestrationDependencyException =
                await Assert.ThrowsAsync<StudentOrchestrationDependencyException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationDependencyException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationDependencyException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.enrollmentServiceMock.Verify(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationDependencyException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOrchestrationServiceExceptionOnOnboardIfEnrollmentServiceExceptionOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student inputStudent = randomStudent;
            Student returnedStudent = randomStudent;

            var failedEnrollmentServiceException =
                new FailedEnrollmentServiceException(
                    message: "Failed enrollment service error occurred, contact support.",
                    innerException: new Xeption(),
                    data: new Hashtable());

            var enrollmentServiceException =
                new EnrollmentServiceException(
                    message: "Enrollment service error occurred, contact support.",
                    innerException: failedEnrollmentServiceException);

            var expectedStudentOrchestrationServiceException =
                new StudentOrchestrationServiceException(
                    message: "Student orchestration service error occurred, contact support.",
                    innerException: enrollmentServiceException);

            this.studentServiceMock.Setup(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedStudent);

            this.enrollmentServiceMock.Setup(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(enrollmentServiceException);

            // when
            ValueTask<Student> onboardStudentTask =
                this.studentOrchestrationService.OnboardStudentAsync(inputStudent);

            StudentOrchestrationServiceException actualStudentOrchestrationServiceException =
                await Assert.ThrowsAsync<StudentOrchestrationServiceException>(
                    onboardStudentTask.AsTask);

            // then
            actualStudentOrchestrationServiceException.Should()
                .BeEquivalentTo(expectedStudentOrchestrationServiceException);

            this.studentServiceMock.Verify(service =>
                service.AddStudentAsync(inputStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.enrollmentServiceMock.Verify(service =>
                service.AddEnrollmentAsync(
                    It.Is<Enrollment>(e => e.StudentId == inputStudent.Id),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[StudentOrchestrationService] Onboarding student {inputStudent.Id}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentOrchestrationServiceException))),
                        Times.Once);

            this.studentServiceMock.VerifyNoOtherCalls();
            this.enrollmentServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
