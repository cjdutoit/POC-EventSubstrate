// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Linq.Expressions;
using Moq;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Services.Foundations.Timetables;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Timetables
{
    public partial class TimetableServiceTests
    {
        private readonly Mock<IEventSubstrateBroker> eventSubstrateBrokerMock;
        private readonly Mock<IEventEnvelopeFactory> envelopeFactoryMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly ITimetableService timetableService;

        public TimetableServiceTests()
        {
            this.eventSubstrateBrokerMock = new Mock<IEventSubstrateBroker>();
            this.envelopeFactoryMock = new Mock<IEventEnvelopeFactory>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();

            this.timetableService = new TimetableService(
                eventSubstrateBroker: this.eventSubstrateBrokerMock.Object,
                envelopeFactory: this.envelopeFactoryMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object);
        }

        private static Guid CreateRandomGuid() => Guid.NewGuid();

        private static EventEnvelope<TimetableGeneratedEvent> CreateTimetableGeneratedEnvelope(
            Guid studentId) =>
            new EventEnvelope<TimetableGeneratedEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = StudentEventNames.TimetableGenerated,
                    Version = 1,
                },
                Content = new TimetableGeneratedEvent { StudentId = studentId }
            };

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException) =>
            actualException => actualException.Message == expectedException.Message
                && actualException.InnerException.Message == expectedException.InnerException.Message;
    }
}
