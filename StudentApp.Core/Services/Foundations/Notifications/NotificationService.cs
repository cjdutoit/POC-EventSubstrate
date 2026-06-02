// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.Emails;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public sealed partial class NotificationService : INotificationService
    {
        private readonly IEmailBroker emailBroker;
        private readonly IEventSubstrateBroker eventSubstrateBroker;
        private readonly IEventEnvelopeFactory envelopeFactory;
        private readonly ILoggingBroker loggingBroker;
        private readonly ISecurityBroker securityBroker;

        public NotificationService(
            IEmailBroker emailBroker,
            IEventSubstrateBroker eventSubstrateBroker,
            IEventEnvelopeFactory envelopeFactory,
            ILoggingBroker loggingBroker,
            ISecurityBroker securityBroker)
        {
            this.emailBroker = emailBroker;
            this.eventSubstrateBroker = eventSubstrateBroker;
            this.envelopeFactory = envelopeFactory;
            this.loggingBroker = loggingBroker;
            this.securityBroker = securityBroker;
        }

        public ValueTask SendWelcomeEmailAsync(
            Guid studentId,
            string email,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateSendWelcomeEmailArguments(studentId, email);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<WelcomeEmailSentEvent> inboundEnvelope =
                    await this.envelopeFactory.CreateAsync(
                        new WelcomeEmailSentEvent { StudentId = studentId },
                        StudentEventNames.WelcomeEmailSent,
                        securityContext,
                        cancellationToken);

                await DoSendWelcomeEmailAsync(inboundEnvelope, cancellationToken);
            });

        public ValueTask SendTimetableEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateSendTimetableEmailArguments(studentId);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<TimetableEmailSentEvent> inboundEnvelope =
                    await this.envelopeFactory.CreateAsync(
                        new TimetableEmailSentEvent { StudentId = studentId },
                        StudentEventNames.TimetableEmailSent,
                        securityContext,
                        cancellationToken);

                await DoSendTimetableEmailAsync(inboundEnvelope, cancellationToken);
            });

        private async ValueTask DoSendWelcomeEmailAsync(
            EventEnvelope<WelcomeEmailSentEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[NotificationService] Sending welcome email to student {inboundEnvelope.Content.StudentId}");

            await this.emailBroker.SendWelcomeEmailAsync(inboundEnvelope.Content.StudentId, cancellationToken);

            EventEnvelope<WelcomeEmailSentEvent> outboundEnvelope =
                new EventEnvelope<WelcomeEmailSentEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.WelcomeEmailSent,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = inboundEnvelope.Content
                };

            this.loggingBroker.LogInformation(
                $"[NotificationService] Welcome email sent to student {outboundEnvelope.Content.StudentId}");

            this.loggingBroker.LogInformation(
                $"[NotificationService] Emitting {outboundEnvelope.Metadata.EventType} event to Substrate " +
                    $"for student {outboundEnvelope.Content.StudentId}");

            await this.eventSubstrateBroker.EmitAsync(outboundEnvelope, cancellationToken);
        }

        private async ValueTask DoSendTimetableEmailAsync(
            EventEnvelope<TimetableEmailSentEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[NotificationService] Sending timetable email to student {inboundEnvelope.Content.StudentId}");

            await this.emailBroker.SendTimetableEmailAsync(inboundEnvelope.Content.StudentId, cancellationToken);

            EventEnvelope<TimetableEmailSentEvent> outboundEnvelope =
                new EventEnvelope<TimetableEmailSentEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.TimetableEmailSent,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = inboundEnvelope.Content
                };

            this.loggingBroker.LogInformation(
                $"[NotificationService] Timetable email sent to student {outboundEnvelope.Content.StudentId}");

            this.loggingBroker.LogInformation(
                $"[NotificationService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for student {outboundEnvelope.Content.StudentId}");

            await this.eventSubstrateBroker.EmitAsync(outboundEnvelope, cancellationToken);
        }
    }
}
