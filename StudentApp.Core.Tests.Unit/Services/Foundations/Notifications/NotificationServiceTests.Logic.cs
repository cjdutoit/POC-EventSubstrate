// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Notifications
{
    public partial class NotificationServiceTests
    {
        [Fact]
        public async Task ShouldSendWelcomeEmailAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();
            string randomEmail = GetRandomString();

            EventEnvelope<WelcomeEmailSentEvent> envelope =
                CreateWelcomeEmailSentEnvelope(randomStudentId);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<WelcomeEmailSentEvent>(),
                    StudentEventNames.WelcomeEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // when
            await this.notificationService.SendWelcomeEmailAsync(
                randomStudentId,
                randomEmail,
                CancellationToken.None);

            // then
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

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<WelcomeEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudentId.ToString()))),
                Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.emailBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSendTimetableEmailAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            EventEnvelope<TimetableEmailSentEvent> envelope =
                CreateTimetableEmailSentEnvelope(randomStudentId);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<TimetableEmailSentEvent>(),
                    StudentEventNames.TimetableEmailSent,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // when
            await this.notificationService.SendTimetableEmailAsync(
                randomStudentId,
                CancellationToken.None);

            // then
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

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<TimetableEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudentId.ToString()))),
                Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.emailBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
