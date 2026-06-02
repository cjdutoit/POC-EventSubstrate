// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public interface IEventReceiver<TEvent>
    {
        ValueTask ReceiveAsync(
            EventEnvelope<TEvent> envelope,
            CancellationToken cancellationToken = default);
    }
}
