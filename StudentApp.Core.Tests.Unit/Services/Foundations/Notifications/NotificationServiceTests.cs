// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Threading;
using Moq;
using StudentApp.Core.Brokers.DateTimes;
using StudentApp.Core.Brokers.Emails;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Serializations;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Services.Foundations.Notifications;
using Tynamix.ObjectFiller;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Notifications
{
    public partial class NotificationServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IEmailBroker> emailBrokerMock;
        private readonly Mock<IEventSubstrateBroker> eventSubstrateBrokerMock;
        private readonly Mock<IEventEnvelopeFactory> envelopeFactoryMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IJsonSerializationBroker> jsonSerializationBrokerMock;
        private readonly INotificationService notificationService;

        public NotificationServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.emailBrokerMock = new Mock<IEmailBroker>();
            this.eventSubstrateBrokerMock = new Mock<IEventSubstrateBroker>();
            this.envelopeFactoryMock = new Mock<IEventEnvelopeFactory>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.jsonSerializationBrokerMock = new Mock<IJsonSerializationBroker>();

            this.storageBrokerMock.Setup(x => x.SelectProcessedEventExistsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            this.jsonSerializationBrokerMock.Setup(x => x.SerializeAsync(It.IsAny<object>()))
                .ReturnsAsync("{}");

            this.notificationService = new NotificationService(
                storageBroker: this.storageBrokerMock.Object,
                emailBroker: this.emailBrokerMock.Object,
                eventSubstrateBroker: this.eventSubstrateBrokerMock.Object,
                envelopeFactory: this.envelopeFactoryMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                jsonSerializationBroker: this.jsonSerializationBrokerMock.Object);
        }

        private static EventEnvelope<WelcomeEmailSentEvent> CreateWelcomeEmailSentEnvelope(
            Guid studentId) =>
            new EventEnvelope<WelcomeEmailSentEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = StudentEventNames.WelcomeEmailSent,
                    Version = 1
                },
                Content = new WelcomeEmailSentEvent { StudentId = studentId }
            };

        private static EventEnvelope<TimetableEmailSentEvent> CreateTimetableEmailSentEnvelope(
            Guid studentId) =>
            new EventEnvelope<TimetableEmailSentEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = StudentEventNames.TimetableEmailSent,
                    Version = 1
                },
                Content = new TimetableEmailSentEvent { StudentId = studentId }
            };

        private static Guid CreateRandomGuid() => Guid.NewGuid();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException) =>
            actualException => actualException.Message == expectedException.Message
                && actualException.InnerException.Message == expectedException.InnerException.Message;
    }
}
