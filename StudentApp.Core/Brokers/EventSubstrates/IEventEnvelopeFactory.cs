// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public interface IEventEnvelopeFactory
    {
        ValueTask<EventEnvelope<TEvent>> CreateAsync<TEvent>(
            TEvent content,
            string eventName,
            SecurityContext securityContext,
            CancellationToken cancellationToken = default);
    }
}
