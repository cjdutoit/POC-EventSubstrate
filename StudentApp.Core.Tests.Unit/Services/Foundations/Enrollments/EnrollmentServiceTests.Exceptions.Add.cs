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
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Enrollments
{
    public partial class EnrollmentServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnAddIfSqlErrorOccursAsync()
        {
            // given
            Enrollment randomEnrollment = CreateRandomEnrollment();
            SqlException sqlException = CreateSqlException();

            var failedEnrollmentStorageException =
                new FailedEnrollmentStorageException(
                    message: "Failed enrollment storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);

            var expectedEnrollmentDependencyException =
                new EnrollmentDependencyException(
                    message: "Enrollment dependency error occurred, contact support.",
                    innerException: failedEnrollmentStorageException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.storageBrokerMock.Setup(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(sqlException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(randomEnrollment);

            EnrollmentDependencyException actualEnrollmentDependencyException =
                await Assert.ThrowsAsync<EnrollmentDependencyException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentDependencyException.Should()
                .BeEquivalentTo(expectedEnrollmentDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Adding enrollment for student {randomEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentDependencyException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDuplicateKeyErrorOccursAsync()
        {
            // given
            Enrollment randomEnrollment = CreateRandomEnrollment();
            string someMessage = GetRandomString();

            var duplicateKeyException =
                new DuplicateKeyException(someMessage);

            var alreadyExistsEnrollmentException =
                new AlreadyExistsEnrollmentException(
                    message: "Enrollment with the same id already exists.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

            var expectedEnrollmentDependencyValidationException =
                new EnrollmentDependencyValidationException(
                    message: "Enrollment dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.storageBrokerMock.Setup(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(duplicateKeyException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(randomEnrollment);

            EnrollmentDependencyValidationException actualEnrollmentDependencyValidationException =
                await Assert.ThrowsAsync<EnrollmentDependencyValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentDependencyValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Adding enrollment for student {randomEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDbUpdateConcurrencyErrorOccursAsync()
        {
            // given
            Enrollment randomEnrollment = CreateRandomEnrollment();

            var dbUpdateConcurrencyException =
                new DbUpdateConcurrencyException();

            var lockedEnrollmentException =
                new LockedEnrollmentException(
                    message: "Locked enrollment record, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);

            var expectedEnrollmentDependencyValidationException =
                new EnrollmentDependencyValidationException(
                    message: "Enrollment dependency validation error occurred, fix the errors and try again.",
                    innerException: lockedEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.storageBrokerMock.Setup(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(dbUpdateConcurrencyException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(randomEnrollment);

            EnrollmentDependencyValidationException actualEnrollmentDependencyValidationException =
                await Assert.ThrowsAsync<EnrollmentDependencyValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentDependencyValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Adding enrollment for student {randomEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentDependencyValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfUnexpectedErrorOccursAsync()
        {
            // given
            Enrollment randomEnrollment = CreateRandomEnrollment();
            var serviceException = new Exception();

            var failedEnrollmentServiceException =
                new FailedEnrollmentServiceException(
                    message: "Failed enrollment service error occurred, contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedEnrollmentServiceException =
                new EnrollmentServiceException(
                    message: "Enrollment service error occurred, contact support.",
                    innerException: failedEnrollmentServiceException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.storageBrokerMock.Setup(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(randomEnrollment);

            EnrollmentServiceException actualEnrollmentServiceException =
                await Assert.ThrowsAsync<EnrollmentServiceException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentServiceException.Should()
                .BeEquivalentTo(expectedEnrollmentServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Adding enrollment for student {randomEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentServiceException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    randomEnrollment,
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
