// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using Xunit;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Notifications
{
    public partial class NotificationServiceTests
    {
        [Fact]
        public async Task ShouldReceiveTimetableGeneratedEventAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            TimetableGeneratedEvent timetableGeneratedEvent = new TimetableGeneratedEvent
            {
                StudentId = randomStudentId
            };

            EventEnvelope<TimetableGeneratedEvent> inputEnvelope =
                new EventEnvelope<TimetableGeneratedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.TimetableGenerated,
                        Version = 1
                    },
                    SecurityContext = new SecurityContext
                    {
                        Username = "test-user"
                    },
                    RequestContext = new RequestContext(),
                    Integrity = new EnvelopeIntegrity(),
                    Content = timetableGeneratedEvent
                };

            EventEnvelope<TimetableEmailSentEvent> expectedTimetableEmailEnvelope =
                CreateTimetableEmailSentEnvelope(randomStudentId);

            this.emailBrokerMock.Setup(broker =>
                broker.SendTimetableEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            this.eventSubstrateBrokerMock.Setup(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<TimetableEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // when
            IEventReceiver<TimetableGeneratedEvent> eventReceiver =
                this.notificationService as IEventReceiver<TimetableGeneratedEvent>;

            await eventReceiver.ReceiveAsync(inputEnvelope, CancellationToken.None);

            // then
            this.emailBrokerMock.Verify(broker =>
                broker.SendTimetableEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<TimetableEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains(StudentEventNames.TimetableGenerated) &&
                        msg.Contains(randomStudentId.ToString()))),
                Times.Once);

            this.emailBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
