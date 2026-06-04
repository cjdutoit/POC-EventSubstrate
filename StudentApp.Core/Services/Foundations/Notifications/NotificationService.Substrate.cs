// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public sealed partial class NotificationService :
        IEventReceiver<StudentAddedEvent>,
        IEventReceiver<TimetableGeneratedEvent>
    {
        async ValueTask IEventReceiver<StudentAddedEvent>.ReceiveAsync(
            EventEnvelope<StudentAddedEvent> envelope,
            CancellationToken cancellationToken)
        {
            await HandleStudentCreatedAsync(envelope, cancellationToken);
        }

        async ValueTask IEventReceiver<TimetableGeneratedEvent>.ReceiveAsync(
            EventEnvelope<TimetableGeneratedEvent> envelope,
            CancellationToken cancellationToken)
        {
            await HandleTimetableGeneratedAsync(envelope, cancellationToken);
        }

        private async ValueTask HandleStudentCreatedAsync(
            EventEnvelope<StudentAddedEvent> envelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[Substrate] Relaying {StudentEventNames.StudentAdded} to NotificationService for student {envelope.Content.StudentId}");

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
        }

        private async ValueTask HandleTimetableGeneratedAsync(
            EventEnvelope<TimetableGeneratedEvent> envelope,
            CancellationToken cancellationToken)
        {
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
        }
    }
}
