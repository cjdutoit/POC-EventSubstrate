// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.ProcessedEvents;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public sealed partial class NotificationService :
        IEventReceiver<StudentAddedEvent>,
        IEventReceiver<TimetableGeneratedEvent>
    {
        ValueTask IEventReceiver<StudentAddedEvent>.ReceiveAsync(
            EventEnvelope<StudentAddedEvent> envelope,
            CancellationToken cancellationToken) =>
        TryCatch(
            async () => await HandleStudentCreatedAsync(envelope, cancellationToken),
            envelope,
            cancellationToken);

        ValueTask IEventReceiver<TimetableGeneratedEvent>.ReceiveAsync(
            EventEnvelope<TimetableGeneratedEvent> envelope,
            CancellationToken cancellationToken) =>
        TryCatch(
            async () => await HandleTimetableGeneratedAsync(envelope, cancellationToken),
            envelope,
            cancellationToken);

        private async ValueTask HandleStudentCreatedAsync(
            EventEnvelope<StudentAddedEvent> envelope,
            CancellationToken cancellationToken)
        {
            ValidateStudentAddedEnvelope(envelope);

            bool alreadyProcessed = await this.storageBroker.SelectProcessedEventExistsAsync(
                envelope.Metadata.EventId,
                nameof(NotificationService),
                cancellationToken);

            if (alreadyProcessed)
                return;

            this.loggingBroker.LogInformation(
                $"[Substrate] Relaying {StudentEventNames.StudentAdded} to NotificationService " +
                    $"for student {envelope.Content.StudentId}");

            EventEnvelope<WelcomeEmailSentEvent> welcomeEnvelope =
                new EventEnvelope<WelcomeEmailSentEvent>
                {
                    Metadata = envelope.Metadata,
                    SecurityContext = envelope.SecurityContext,
                    RequestContext = envelope.RequestContext,
                    Integrity = envelope.Integrity,
                    Content = new WelcomeEmailSentEvent { StudentId = envelope.Content.StudentId }
                };

            await DoSendWelcomeEmailAsync(welcomeEnvelope, cancellationToken);

            await this.storageBroker.InsertProcessedEventAsync(
                new ProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = envelope.Metadata.EventId,
                    ReceiverName = nameof(NotificationService),
                    ProcessedAt = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync()
                },
                cancellationToken);
        }

        private async ValueTask HandleTimetableGeneratedAsync(
            EventEnvelope<TimetableGeneratedEvent> envelope,
            CancellationToken cancellationToken)
        {
            ValidateTimetableGeneratedEnvelope(envelope);

            bool alreadyProcessed = await this.storageBroker.SelectProcessedEventExistsAsync(
                envelope.Metadata.EventId,
                nameof(NotificationService),
                cancellationToken);

            if (alreadyProcessed)
                return;

            this.loggingBroker.LogInformation(
                $"[Substrate] Relaying {StudentEventNames.TimetableGenerated} to NotificationService for student {envelope.Content.StudentId}");

            EventEnvelope<TimetableEmailSentEvent> timetableEmailEnvelope =
                new EventEnvelope<TimetableEmailSentEvent>
                {
                    Metadata = envelope.Metadata,
                    SecurityContext = envelope.SecurityContext,
                    RequestContext = envelope.RequestContext,
                    Integrity = envelope.Integrity,
                    Content = new TimetableEmailSentEvent { StudentId = envelope.Content.StudentId }
                };

            await DoSendTimetableEmailAsync(timetableEmailEnvelope, cancellationToken);

            await this.storageBroker.InsertProcessedEventAsync(
                new ProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = envelope.Metadata.EventId,
                    ReceiverName = nameof(NotificationService),
                    ProcessedAt = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync()
                },
                cancellationToken);
        }
    }
}
