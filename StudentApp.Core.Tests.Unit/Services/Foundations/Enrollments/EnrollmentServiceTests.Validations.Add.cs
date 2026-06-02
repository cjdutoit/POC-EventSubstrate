// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Enrollments
{
    public partial class EnrollmentServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfEnrollmentIsNullAsync()
        {
            // given
            Enrollment nullEnrollment = null;

            var nullEnrollmentException =
                new NullEnrollmentException(message: "Enrollment is null.");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: nullEnrollmentException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(nullEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    It.IsAny<Enrollment>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfEnrollmentIsInvalidAsync()
        {
            // given
            var invalidEnrollment = new Enrollment
            {
                Id = Guid.Empty,
                StudentId = Guid.Empty,
                CourseCode = string.Empty
            };

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.Id),
                value: "Id is required");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.StudentId),
                value: "Id is required");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CourseCode),
                value: "Text is required");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(
                    It.IsAny<Enrollment>(),
                    It.IsAny<CancellationToken>()),
                        Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
