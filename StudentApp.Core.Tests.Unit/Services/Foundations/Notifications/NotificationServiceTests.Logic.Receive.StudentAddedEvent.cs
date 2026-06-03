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
        public async Task ShouldReceiveStudentAddedEventAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            StudentAddedEvent studentAddedEvent = new StudentAddedEvent
            {
                StudentId = randomStudentId,
                FirstName = GetRandomString(),
                LastName = GetRandomString(),
                Email = GetRandomString(),
                Status = string.Empty
            };

            EventEnvelope<StudentAddedEvent> inputEnvelope =
                new EventEnvelope<StudentAddedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentAdded,
                        Version = 1
                    },
                    SecurityContext = new SecurityContext
                    {
                        Username = "test-user"
                    },
                    RequestContext = new RequestContext(),
                    Integrity = new EnvelopeIntegrity(),
                    Content = studentAddedEvent
                };

            EventEnvelope<WelcomeEmailSentEvent> expectedWelcomeEnvelope =
                CreateWelcomeEmailSentEnvelope(randomStudentId);

            this.emailBrokerMock.Setup(broker =>
                broker.SendWelcomeEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            this.eventSubstrateBrokerMock.Setup(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<WelcomeEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // when
            IEventReceiver<StudentAddedEvent> eventReceiver =
                this.notificationService as IEventReceiver<StudentAddedEvent>;

            await eventReceiver.ReceiveAsync(inputEnvelope, CancellationToken.None);

            // then
            this.emailBrokerMock.Verify(broker =>
                broker.SendWelcomeEmailAsync(
                    randomStudentId,
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<WelcomeEmailSentEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains(StudentEventNames.StudentAdded) &&
                        msg.Contains(randomStudentId.ToString()))),
                Times.Once);

            this.emailBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
