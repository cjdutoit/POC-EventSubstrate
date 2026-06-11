// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.ProcessedEvents;

namespace StudentApp.Core.Brokers.Storages
{
    public partial interface IStorageBroker
    {
        ValueTask<bool> SelectProcessedEventExistsAsync(
            Guid eventId,
            string receiverName,
            CancellationToken cancellationToken = default);

        ValueTask<ProcessedEvent> InsertProcessedEventAsync(
            ProcessedEvent processedEvent,
            CancellationToken cancellationToken = default);
    }
}
