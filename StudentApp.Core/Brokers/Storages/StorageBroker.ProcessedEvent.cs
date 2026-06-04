// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore;
using StudentApp.Core.Models.Foundations.ProcessedEvents;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        public async ValueTask<bool> SelectProcessedEventExistsAsync(
            Guid eventId,
            string receiverName,
            CancellationToken cancellationToken = default) =>
            await this.ProcessedEvents
                .AsNoTracking()
                .AnyAsync(
                    pe => pe.EventId == eventId && pe.ReceiverName == receiverName,
                    cancellationToken);

        public async ValueTask<ProcessedEvent> InsertProcessedEventAsync(
            ProcessedEvent processedEvent,
            CancellationToken cancellationToken = default) =>
            await InsertAsync(processedEvent, cancellationToken);
    }
}
