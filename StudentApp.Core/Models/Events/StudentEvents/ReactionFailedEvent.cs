// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events.StudentEvents
{
    public sealed class ReactionFailedEvent
    {
        public Guid OriginalEventId { get; init; }
        public string OriginalEventType { get; init; } = string.Empty;
        public string OriginalEventPayload { get; init; } = string.Empty;
        public string ReceiverServiceName { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
        public string? ErrorStackTrace { get; init; }
        public DateTimeOffset FailedAt { get; init; }
    }
}
