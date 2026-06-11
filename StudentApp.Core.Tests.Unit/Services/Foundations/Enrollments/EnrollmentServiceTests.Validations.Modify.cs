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
        public async Task ShouldThrowValidationExceptionOnModifyIfEnrollmentIsNullAsync()
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
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync((Enrollment)null);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(nullEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfEnrollmentIsInvalidAsync(string invalidText)
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

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: $"Date is the same as {nameof(Enrollment.CreatedWhen)}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(invalidText);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfEnrollmentNotFoundAsync()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateRandomModifyEnrollment(randomDateTimeOffset, randomUserId);
            Enrollment inputEnrollment = randomEnrollment;
            Enrollment noEnrollment = null;

            var notFoundEnrollmentException =
                new NotFoundEnrollmentException(
                    message: $"Could not find enrollment with id: {inputEnrollment.Id}.");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: notFoundEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(inputEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectEnrollmentByIdAsync(
                    inputEnrollment.Id,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(noEnrollment);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(inputEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
                Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectEnrollmentByIdAsync(
                    inputEnrollment.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedEnrollmentValidationException))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedByIsNotSameAsCurrentUserIdAsync()
        {
            // given
            string randomUserId = GetRandomString();
            string differentUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateRandomModifyEnrollment(randomDateTimeOffset, randomUserId);
            Enrollment invalidEnrollment = randomEnrollment;
            invalidEnrollment.UpdatedBy = differentUserId;

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedBy),
                value: $"Expected value to be '{randomUserId}' but found '{differentUserId}'.");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
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
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedWhenIsSameAsCreatedWhenAsync()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateRandomModifyEnrollment(randomDateTimeOffset, randomUserId);
            Enrollment invalidEnrollment = randomEnrollment;
            DateTimeOffset currentDateTime = randomDateTimeOffset;
            DateTimeOffset startDate = currentDateTime.AddSeconds(-90);
            DateTimeOffset endDate = currentDateTime;
            invalidEnrollment.UpdatedWhen = invalidEnrollment.CreatedWhen;

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: $"Date is the same as {nameof(Enrollment.CreatedWhen)}");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidEnrollment.UpdatedWhen}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTime);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
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
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdatedWhenIsNotRecentAsync(int minutes)
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment randomEnrollment = CreateRandomModifyEnrollment(randomDateTimeOffset, randomUserId);
            Enrollment invalidEnrollment = randomEnrollment;
            DateTimeOffset currentDateTime = randomDateTimeOffset;
            DateTimeOffset startDate = currentDateTime.AddSeconds(-90);
            DateTimeOffset endDate = currentDateTime;
            invalidEnrollment.UpdatedWhen = randomDateTimeOffset.AddMinutes(minutes);

            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            invalidEnrollmentException.UpsertDataList(
                key: nameof(Enrollment.UpdatedWhen),
                value: $"Date is not recent. Expected a value between {startDate} and {endDate} but found {invalidEnrollment.UpdatedWhen}");

            var expectedEnrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: invalidEnrollmentException);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(currentDateTime);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
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
                broker.UpdateEnrollmentAsync(
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
        public async Task ShouldThrowValidationExceptionOnModifyIfEnrollmentExceedsMaxLengthAsync()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            Enrollment invalidEnrollment = CreateRandomModifyEnrollment(randomDateTimeOffset, randomUserId);

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
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()))
                    .ReturnsAsync(invalidEnrollment);

            this.securityBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<Enrollment> modifyEnrollmentTask =
                this.enrollmentService.ModifyEnrollmentAsync(invalidEnrollment);

            EnrollmentValidationException actualEnrollmentValidationException =
                await Assert.ThrowsAsync<EnrollmentValidationException>(
                    modifyEnrollmentTask.AsTask);

            // then
            actualEnrollmentValidationException.Should()
                .BeEquivalentTo(expectedEnrollmentValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyModifyAuditValuesAsync(It.IsAny<Enrollment>()),
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
                broker.UpdateEnrollmentAsync(
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





