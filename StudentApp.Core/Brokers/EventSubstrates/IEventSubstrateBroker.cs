// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public partial interface IEventSubstrateBroker
    {
        ValueTask EmitAsync<TEvent>(
            EventEnvelope<TEvent> envelope,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<EventEnvelope<TEvent>> ReplayAsync<TEvent>(
            DateTimeOffset fromDate,
            DateTimeOffset? toDate = null,
            CancellationToken cancellationToken = default);
    }
}
