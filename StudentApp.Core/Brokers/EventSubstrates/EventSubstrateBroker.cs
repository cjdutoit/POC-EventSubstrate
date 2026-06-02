// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public sealed partial class EventSubstrateBroker : IEventSubstrateBroker
    {
        private readonly ILeVentClientRegistry clientRegistry;

        public EventSubstrateBroker(ILeVentClientRegistry clientRegistry)
        {
            this.clientRegistry = clientRegistry;
        }

        public async ValueTask EmitAsync<TContent>(
            EventEnvelope<TContent> envelope,
            CancellationToken cancellationToken = default)
        {
            await this.clientRegistry
                .GetOrCreate<TContent>()
                .PublishEventAsync(envelope, envelope.Metadata.EventType);
        }

        public IAsyncEnumerable<EventEnvelope<TContent>> ReplayAsync<TContent>(
            DateTimeOffset fromDate,
            DateTimeOffset? toDate = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "Replay is not available until durable event storage is enabled.");
        }
    }
}
