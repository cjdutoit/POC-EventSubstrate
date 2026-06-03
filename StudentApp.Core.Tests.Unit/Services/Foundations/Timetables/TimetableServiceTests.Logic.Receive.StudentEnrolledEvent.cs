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

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Timetables
{
    public partial class TimetableServiceTests
    {
        [Fact]
        public async Task ShouldReceiveStudentEnrolledEventAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            StudentEnrolledEvent studentEnrolledEvent = new StudentEnrolledEvent
            {
                StudentId = randomStudentId
            };

            EventEnvelope<StudentEnrolledEvent> inputEnvelope =
                new EventEnvelope<StudentEnrolledEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentEnrolled,
                        Version = 1
                    },
                    SecurityContext = new SecurityContext
                    {
                        Username = "test-user"
                    },
                    RequestContext = new RequestContext(),
                    Integrity = new EnvelopeIntegrity(),
                    Content = studentEnrolledEvent
                };

            EventEnvelope<TimetableGeneratedEvent> expectedTimetableEnvelope =
                CreateTimetableGeneratedEnvelope(randomStudentId);

            this.eventSubstrateBrokerMock.Setup(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<TimetableGeneratedEvent>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // when
            IEventReceiver<StudentEnrolledEvent> eventReceiver =
                this.timetableService as IEventReceiver<StudentEnrolledEvent>;

            await eventReceiver.ReceiveAsync(inputEnvelope, CancellationToken.None);

            // then
            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.Is<EventEnvelope<TimetableGeneratedEvent>>(envelope =>
                        envelope.Content.StudentId == randomStudentId),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains(StudentEventNames.StudentEnrolled) &&
                        msg.Contains(randomStudentId.ToString()))),
                Times.Once);

            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
