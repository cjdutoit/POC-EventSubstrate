// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events
{
    /// <summary>
    /// A generic wrapper that carries a domain event payload alongside the security context,
    /// request context, and event metadata required to process it safely and consistently.
    /// Decouples orchestration services and event handlers from <c>HttpContext</c>,
    /// <c>IHttpContextAccessor</c>, <c>ClaimsPrincipal</c>, and raw JWT tokens, making the
    /// event pipeline compatible with both in-process and future disconnected processing.
    /// </summary>
    /// <typeparam name="T">The type of the domain event content payload.</typeparam>
    public sealed class EventEnvelope<T>
    {
        /// <summary>
        /// The name of the event, used for routing and dispatching to handlers.
        /// </summary>
        public string EventName => this.Metadata.EventType;

        public DateTimeOffset OccurredAt => this.Metadata.OccurredAt;

        /// <summary>
        /// The business payload of the event, such as the domain entity that was created, updated, or deleted.
        /// </summary>
        public T Content { get; init; }

        /// <summary>
        /// The normalized identity of the authenticated caller at the time the event was created.
        /// </summary>
        public SecurityContext SecurityContext { get; init; }

        /// <summary>
        /// Operational information about the originating request or process, used for tracing and audit.
        /// </summary>
        public RequestContext RequestContext { get; init; }

        /// <summary>
        /// Metadata describing the event instance, including its identifier, type, version, and causation.
        /// </summary>
        public EventMetadata Metadata { get; init; }

        public EnvelopeIntegrity Integrity { get; init; }
    }
}