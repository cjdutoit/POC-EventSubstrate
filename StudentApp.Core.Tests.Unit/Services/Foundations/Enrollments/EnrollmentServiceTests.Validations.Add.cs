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
            string randomUserId = GetRandomString();

            var nullEnrollmentException =
                new NullEnrollmentException(message: "Enrollment is null.");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: nullEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync((Enrollment)null);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(nullEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnAddIfEnrollmentIsInvalidAsync(string invalidText)
        {
            // given
            var invalidEnrollment = new Enrollment
            {
                Id = Guid.Empty,
                StudentId = Guid.Empty,
                CourseCode = invalidText,
                CreatedBy = invalidText,
                UpdatedBy = invalidText,
                CreatedWhen = default,
                UpdatedWhen = default
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

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CreatedBy),
                value: "Text is required");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedBy),
                value: "Text is required");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CreatedWhen),
                value: "Date is required");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: "Date is required");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(invalidText);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfUpdatedWhenIsNotSameAsCreatedWhenAsync()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateEnrollmentFiller(randomDateTimeOffset, randomUserId).Create();
            Enrollment invalidEnrollment = randomEnrollment;
            invalidEnrollment.UpdatedWhen = GetRandomDateTimeOffset();

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: $"Date is not the same as {nameof(Enrollment.CreatedWhen)}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedByIsNotSameAsCurrentUserIdAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string differentUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateEnrollmentFiller(randomDateTimeOffset, randomUserId).Create();
            Enrollment invalidEnrollment = randomEnrollment;
            invalidEnrollment.CreatedBy = differentUserId;
            invalidEnrollment.UpdatedBy = differentUserId;

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CreatedBy),
                value: $"Expected value to be '{randomUserId}' but found '{differentUserId}'.");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfUpdatedByIsNotSameAsCreatedByAsync()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateEnrollmentFiller(randomDateTimeOffset, randomUserId).Create();
            Enrollment invalidEnrollment = randomEnrollment;
            invalidEnrollment.UpdatedBy = GetRandomString();

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedBy),
                value: $"Text is not the same as {nameof(Enrollment.CreatedBy)}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(MinutesBeforeOrAfter))]
        public async Task ShouldThrowValidationExceptionOnAddIfCreatedWhenIsNotRecentAsync(int minutes)
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateEnrollmentFiller(randomDateTimeOffset, randomUserId).Create();
            Enrollment invalidEnrollment = randomEnrollment;
            DateTimeOffset currentDateTime = randomDateTimeOffset;
            DateTimeOffset startDate = currentDateTime.AddSeconds(-90);
            DateTimeOffset endDate = currentDateTime;
            invalidEnrollment.CreatedWhen = randomDateTimeOffset.AddMinutes(minutes);
            invalidEnrollment.UpdatedWhen = invalidEnrollment.CreatedWhen;

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CreatedWhen),
                value: $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidEnrollment.CreatedWhen}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTime);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfEnrollmentExceedsMaxLengthAsync()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment invalidEnrollment = CreateEnrollmentFiller(randomDateTimeOffset, randomUserId).Create();

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.CreatedBy),
                value: $"Text exceed max length of {invalidEnrollment.CreatedBy.Length - 1} characters");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedBy),
                value: $"Text exceed max length of {invalidEnrollment.UpdatedBy.Length - 1} characters");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> addEnrollmentTask =
                this.enrollmentService.AddEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    addEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

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
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}





