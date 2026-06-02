// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public sealed class EventEnvelopeFactory : IEventEnvelopeFactory
    {
        public ValueTask<EventEnvelope<TEvent>> CreateAsync<TEvent>(
            TEvent content,
            string eventName,
            SecurityContext securityContext,
            CancellationToken cancellationToken = default)
        {
            var envelope = new EventEnvelope<TEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = eventName,
                    Version = 1,
                    CausationId = null,
                    ParentCorrelationId = null,
                },
                SecurityContext = securityContext,
                RequestContext = new RequestContext { SourceSystem = "StudentApp" },
                Integrity = new EnvelopeIntegrity
                {
                    Algorithm = "HMACSHA256",
                    Signature = "stub-signature",
                    SignedDate = DateTimeOffset.UtcNow
                },
                Content = content
            };

            return ValueTask.FromResult(envelope);
        }
    }
}
