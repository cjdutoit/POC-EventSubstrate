// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Notifications.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Notifications
{
    public partial class NotificationServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnSendWelcomeEmailIfUnexpectedErrorOccursAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();
            string randomEmail = GetRandomString();
            var serviceException = new Exception();

            var failedNotificationServiceException =
                new FailedNotificationServiceException(
                    message: "Failed notification service error occurred, contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedNotificationServiceException =
                new NotificationServiceException(
                    message: "Notification service error occurred, contact support.",
                    innerException: failedNotificationServiceException);

            this.envelopeFactoryMock
                .Setup(factory => factory.CreateAsync(
                    It.IsAny<WelcomeEmailSentEvent>(),
                    StudentEventNames.WelcomeEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateWelcomeEmailSentEnvelope(randomStudentId));

            this.emailBrokerMock
                .Setup(broker => broker.SendWelcomeEmailAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask sendTask =
                this.notificationService.SendWelcomeEmailAsync(
                    randomStudentId,
                    randomEmail,
                    CancellationToken.None);

            NotificationServiceException actualException =
                await Assert.ThrowsAsync<NotificationServiceException>(
                    sendTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedNotificationServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedNotificationServiceException))),
                Times.Once);

            this.emailBrokerMock.Verify(broker =>
                broker.SendWelcomeEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<WelcomeEmailSentEvent>(),
                    StudentEventNames.WelcomeEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudentId.ToString()))),
                Times.AtLeastOnce);

            this.emailBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnSendTimetableEmailIfUnexpectedErrorOccursAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();
            var serviceException = new Exception();

            var failedNotificationServiceException =
                new FailedNotificationServiceException(
                    message: "Failed notification service error occurred, contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedNotificationServiceException =
                new NotificationServiceException(
                    message: "Notification service error occurred, contact support.",
                    innerException: failedNotificationServiceException);

            this.envelopeFactoryMock
                .Setup(factory => factory.CreateAsync(
                    It.IsAny<TimetableEmailSentEvent>(),
                    StudentEventNames.TimetableEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTimetableEmailSentEnvelope(randomStudentId));

            this.emailBrokerMock
                .Setup(broker => broker.SendTimetableEmailAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask sendTask =
                this.notificationService.SendTimetableEmailAsync(
                    randomStudentId,
                    CancellationToken.None);

            NotificationServiceException actualException =
                await Assert.ThrowsAsync<NotificationServiceException>(
                    sendTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedNotificationServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedNotificationServiceException))),
                Times.Once);

            this.emailBrokerMock.Verify(broker =>
                broker.SendTimetableEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<TimetableEmailSentEvent>(),
                    StudentEventNames.TimetableEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudentId.ToString()))),
                Times.AtLeastOnce);

            this.emailBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
