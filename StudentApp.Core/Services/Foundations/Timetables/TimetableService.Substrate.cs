// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.ProcessedEvents;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public sealed partial class TimetableService :
        IEventReceiver<StudentEnrolledEvent>
    {
        ValueTask IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken) =>
        TryCatch(
            async () => await HandleStudentEnrolledAsync(envelope, cancellationToken),
            envelope,
            cancellationToken);

        private async ValueTask HandleStudentEnrolledAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken)
        {
            ValidateEnvelopeIsNotNull(envelope);
            ValidateEnvelopeContent(envelope);

            bool alreadyProcessed = await this.storageBroker.SelectProcessedEventExistsAsync(
                envelope.Metadata.EventId,
                nameof(TimetableService),
                cancellationToken);

            if (alreadyProcessed)
                return;

            this.loggingBroker.LogInformation(
                $"[Substrate] Relaying {StudentEventNames.StudentEnrolled} to TimetableService " +
                    $"for student {envelope.Content.StudentId}");

            EventEnvelope<TimetableGeneratedEvent> timetableEnvelope =
                new EventEnvelope<TimetableGeneratedEvent>
                {
                    Metadata = envelope.Metadata,
                    SecurityContext = envelope.SecurityContext,
                    RequestContext = envelope.RequestContext,
                    Integrity = envelope.Integrity,
                    Content = new TimetableGeneratedEvent { StudentId = envelope.Content.StudentId }
                };

            await DoGenerateTimetableAsync(timetableEnvelope, cancellationToken);

            await this.storageBroker.InsertProcessedEventAsync(
                new ProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = envelope.Metadata.EventId,
                    ReceiverName = nameof(TimetableService),
                    ProcessedAt = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync()
                },
                cancellationToken);
        }
    }
}
