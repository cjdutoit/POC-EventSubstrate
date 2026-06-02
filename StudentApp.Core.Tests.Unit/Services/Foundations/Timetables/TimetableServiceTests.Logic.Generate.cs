// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Timetables
{
    public partial class TimetableServiceTests
    {
        [Fact]
        public async Task ShouldGenerateTimetableAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();

            EventEnvelope<TimetableGeneratedEvent> envelope =
                CreateTimetableGeneratedEnvelope(randomStudentId);

            this.envelopeFactoryMock
                .Setup(factory => factory.CreateAsync(
                    It.IsAny<TimetableGeneratedEvent>(),
                    StudentEventNames.TimetableGenerated,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // when
            await this.timetableService.GenerateTimetableAsync(
                randomStudentId,
                CancellationToken.None);

            // then
            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<TimetableGeneratedEvent>(),
                    StudentEventNames.TimetableGenerated,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<TimetableGeneratedEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(randomStudentId.ToString()))),
                Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
