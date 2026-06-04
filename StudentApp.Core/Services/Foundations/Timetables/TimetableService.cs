// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.DateTimes;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Serializations;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public sealed partial class TimetableService : ITimetableService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IEventSubstrateBroker eventSubstrateBroker;
        private readonly IEventEnvelopeFactory envelopeFactory;
        private readonly ILoggingBroker loggingBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IJsonSerializationBroker jsonSerializationBroker;

        public TimetableService(
            IStorageBroker storageBroker,
            IEventSubstrateBroker eventSubstrateBroker,
            IEventEnvelopeFactory envelopeFactory,
            ILoggingBroker loggingBroker,
            ISecurityBroker securityBroker,
            IDateTimeBroker dateTimeBroker,
            IJsonSerializationBroker jsonSerializationBroker)
        {
            this.storageBroker = storageBroker;
            this.eventSubstrateBroker = eventSubstrateBroker;
            this.envelopeFactory = envelopeFactory;
            this.loggingBroker = loggingBroker;
            this.securityBroker = securityBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.jsonSerializationBroker = jsonSerializationBroker;
        }

        public ValueTask GenerateTimetableAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateGenerateTimetableArguments(studentId);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<TimetableGeneratedEvent> inboundEnvelope =
                    await this.envelopeFactory.CreateAsync(
                        new TimetableGeneratedEvent { StudentId = studentId },
                        StudentEventNames.TimetableGenerated,
                        securityContext,
                        cancellationToken);

                await DoGenerateTimetableAsync(inboundEnvelope, cancellationToken);
            });

        private async ValueTask DoGenerateTimetableAsync(
            EventEnvelope<TimetableGeneratedEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[TimetableService] Generating timetable for student {inboundEnvelope.Content.StudentId}");

            EventEnvelope<TimetableGeneratedEvent> outboundEnvelope =
                new EventEnvelope<TimetableGeneratedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.TimetableGenerated,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = inboundEnvelope.Content
                };

            this.loggingBroker.LogInformation(
                $"[TimetableService] Timetable generated for student {outboundEnvelope.Content.StudentId}");

            this.loggingBroker.LogInformation(
                $"[TimetableService] Emitting {outboundEnvelope.Metadata.EventType} event to Substrate " +
                    $"for student {outboundEnvelope.Content.StudentId}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);
        }
    }
}
