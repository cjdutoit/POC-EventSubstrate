// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public sealed partial class TimetableService :
        IEventReceiver<StudentEnrolledEvent>
    {
        async ValueTask IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken)
        {
            await HandleStudentEnrolledAsync(envelope, cancellationToken);
        }

        private async ValueTask HandleStudentEnrolledAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken)
        {
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
        }
    }
}
