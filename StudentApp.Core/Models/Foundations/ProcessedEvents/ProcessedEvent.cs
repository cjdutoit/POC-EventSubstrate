// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Foundations.ProcessedEvents
{
    public sealed class ProcessedEvent
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public DateTimeOffset ProcessedAt { get; set; }
    }
}
