// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Foundations.Students.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRemoveIfSqlErrorOccursAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            SqlException sqlException = CreateSqlException();

            var failedStudentStorageException =
                new FailedStudentStorageException(
                    message: "Failed student storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);

            var expectedStudentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: failedStudentStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(sqlException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(randomStudentId);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    removeStudentByIdTask.AsTask);

            // then
            actualStudentDependencyException.Should()
                .BeEquivalentTo(expectedStudentDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveIfDbUpdateConcurrencyErrorOccursAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            Student randomStudent = CreateRandomStudent();
            var dbUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedStudentException =
                new LockedStudentException(
                    message: "Locked student record, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: lockedStudentException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(randomStudent);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyRemoveAuditValuesAsync(randomStudent))
                    .ReturnsAsync(randomStudent);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentRemovedEvent>(),
                    StudentEventNames.StudentRemoved,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentRemovedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(randomStudent.Id);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    removeStudentByIdTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentRemovedEvent>(),
                    StudentEventNames.StudentRemoved,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyRemoveAuditValuesAsync(randomStudent),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudent.Id.ToString()))),
                Times.AtLeastOnce);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRemoveIfUnexpectedErrorOccursAsync()
        {
            // given
            Guid randomStudentId = Guid.NewGuid();
            var serviceException = new Exception();

            var failedStudentServiceException =
                new FailedStudentServiceException(
                    message: "Failed student service error occurred, contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedStudentServiceException =
                new StudentServiceException(
                    message: "Student service error occurred, contact support.",
                    innerException: failedStudentServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> removeStudentByIdTask =
                this.studentService.RemoveStudentByIdAsync(randomStudentId);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    removeStudentByIdTask.AsTask);

            // then
            actualStudentServiceException.Should()
                .BeEquivalentTo(expectedStudentServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
