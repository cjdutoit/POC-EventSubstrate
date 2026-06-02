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
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAsync()
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

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentModifiedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(sqlException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(randomStudent);

            StudentDependencyException actualStudentDependencyException =
                await Assert.ThrowsAsync<StudentDependencyException>(
                    modifyStudentTask.AsTask);

            // then
            actualStudentDependencyException.Should()
                .BeEquivalentTo(expectedStudentDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudent.Id.ToString()))),
                Times.AtLeastOnce);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDbUpdateConcurrencyErrorOccursAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Student storageStudent = randomStudent;
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

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentModifiedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(storageStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(randomStudent);

            StudentDependencyValidationException actualStudentDependencyValidationException =
                await Assert.ThrowsAsync<StudentDependencyValidationException>(
                    modifyStudentTask.AsTask);

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
                broker.UpdateStudentAsync(
                    randomStudent,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudent.Id.ToString()))),
                Times.AtLeastOnce);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfUnexpectedErrorOccursAsync()
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

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateStudentModifiedEnvelope(randomStudent));

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask<Student> modifyStudentTask =
                this.studentService.ModifyStudentAsync(randomStudent);

            StudentServiceException actualStudentServiceException =
                await Assert.ThrowsAsync<StudentServiceException>(
                    modifyStudentTask.AsTask);

            // then
            actualStudentServiceException.Should()
                .BeEquivalentTo(expectedStudentServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedStudentServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentModifiedEvent>(),
                    StudentEventNames.StudentModified,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudent.Id.ToString()))),
                Times.AtLeastOnce);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
