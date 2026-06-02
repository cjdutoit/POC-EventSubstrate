// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
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
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
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

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent))
                    .ReturnsAsync(randomStudent);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentAddedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(sqlException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(randomStudent);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    addStudentTask.AsTask);

            // then
            actualStudentDependencyException.Should()
                .BeEquivalentTo(expectedStudentDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDuplicateKeyErrorOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            string someMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(someMessage);

            var alreadyExistsStudentException =
                new AlreadyExistsStudentException(
                    message: "Student with the same id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsStudentException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent))
                    .ReturnsAsync(randomStudent);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentAddedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(randomStudent);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    addStudentTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDbUpdateConcurrencyErrorOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();

            var dbUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedStudentException =
                new LockedStudentException(
                    message: "Locked student record, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);

            var expectedStudentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: lockedStudentException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent))
                    .ReturnsAsync(randomStudent);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentAddedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(randomStudent);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    addStudentTask.AsTask);

            // then
            actualStudentDependencyValidationException.Should()
                .BeEquivalentTo(expectedStudentDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent),
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
        public async Task ShouldThrowServiceExceptionOnAddIfUnexpectedErrorOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
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

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent))
                    .ReturnsAsync(randomStudent);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentAddedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> addStudentTask =
                this.studentService.AddStudentAsync(randomStudent);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    addStudentTask.AsTask);

            // then
            actualStudentServiceException.Should()
                .BeEquivalentTo(expectedStudentServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentAddedEvent>(),
                    StudentEventNames.StudentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(randomStudent),
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
    }
}
