// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Notifications.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Notifications
{
    public partial class NotificationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnSendWelcomeEmailIfStudentIdIsEmptyAsync()
        {
            // given
            Guid emptyStudentId = Guid.Empty;
            string randomEmail = GetRandomString();

            var invalidNotificationException =
                new InvalidNotificationException(
                    message: "Invalid notification. Please correct the errors and try again.");

            invalidNotificationException.UpsertDataList(
                key: "studentId",
                value: "Id is required");

            var expectedNotificationValidationException =
                new NotificationValidationException(
                    message: "Notification validation error occurred, fix the errors and try again.",
                    innerException: invalidNotificationException);

            // when
            ValueTask sendTask =
                this.notificationService.SendWelcomeEmailAsync(
                    emptyStudentId,
                    randomEmail,
                    CancellationToken.None);

            NotificationValidationException actualException =
                await Assert.ThrowsAsync<NotificationValidationException>(
                    sendTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedNotificationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedNotificationValidationException))),
                Times.Once);

            this.emailBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnSendWelcomeEmailIfEmailIsInvalidAsync(
            string invalidEmail)
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            var invalidNotificationException =
                new InvalidNotificationException(
                    message: "Invalid notification. Please correct the errors and try again.");

            invalidNotificationException.UpsertDataList(
                key: "email",
                value: "Text is required");

            var expectedNotificationValidationException =
                new NotificationValidationException(
                    message: "Notification validation error occurred, fix the errors and try again.",
                    innerException: invalidNotificationException);

            // when
            ValueTask sendTask =
                this.notificationService.SendWelcomeEmailAsync(
                    randomStudentId,
                    invalidEmail,
                    CancellationToken.None);

            NotificationValidationException actualException =
                await Assert.ThrowsAsync<NotificationValidationException>(
                    sendTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedNotificationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedNotificationValidationException))),
                Times.Once);

            this.emailBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnSendTimetableEmailIfStudentIdIsEmptyAsync()
        {
            // given
            Guid emptyStudentId = Guid.Empty;

            var invalidNotificationException =
                new InvalidNotificationException(
                    message: "Invalid notification. Please correct the errors and try again.");

            invalidNotificationException.UpsertDataList(
                key: "studentId",
                value: "Id is required");

            var expectedNotificationValidationException =
                new NotificationValidationException(
                    message: "Notification validation error occurred, fix the errors and try again.",
                    innerException: invalidNotificationException);

            // when
            ValueTask sendTask =
                this.notificationService.SendTimetableEmailAsync(
                    emptyStudentId,
                    CancellationToken.None);

            NotificationValidationException actualException =
                await Assert.ThrowsAsync<NotificationValidationException>(
                    sendTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedNotificationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedNotificationValidationException))),
                Times.Once);

            this.emailBrokerMock.VerifyNoOtherCalls();
        }
    }
}
