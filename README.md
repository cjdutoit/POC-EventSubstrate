# Event Substrate

## 1. Overview

The Event Substrate is a shared event broker that enables services to communicate and collaborate through events while remaining independent of one another.

In this model, every service is both:

- **An Event Emitter** – capable of publishing events when something happens.
- **An Event Receiver** – capable of subscribing to and processing events from other services.

This creates a loosely coupled system where services collaborate through a shared event substrate instead of calling each other directly.

This approach allows for greater flexibility, scalability, and maintainability as the system evolves over time.

It is not a replacement for a traditional service architecture, but rather an additional layer that enables event-driven communication between services.

---

## 2. The Core Idea

### 2.1 Traditional Service Coordination

Traditional business workflows are coordinated by orchestration services:

```text
API
 |
 v
Student Orchestration Service
 |
 +--> Student Service
 |
 +--> Enrollment Service
 |
 +--> Notification Service
```

The orchestration service is responsible for coordinating the workflow and determining which services should be called and in what order.

As the system grows, orchestration services can become increasingly complex because they must understand every downstream dependency involved in a business process.

### 2.2 Event-Driven Coordination

With the Event Substrate, services communicate through events rather than requiring orchestration services to directly invoke every participant.

```text
Student Service
 |
 +--> StudentCreated Event
         |
         +--> Enrollment Service
         |
         +--> Notification Service
         |
         +--> Audit Service
```

The service that publishes the event does not need to know which services will consume it.

New consumers can subscribe to existing events without requiring changes to the publisher.

### 2.3 The Result

Instead of:

```text
Orchestration Service
 |
 +--> Service A
 |
 +--> Service B
 |
 +--> Service C
 |
 +--> Service D
```

The orchestration service can focus on initiating the business process while the Event Substrate distributes events to interested subscribers.

```text
Orchestration Service
 |
 +--> Service A
         |
         +--> Event Substrate
                   |
                   +--> Service B
                   |
                   +--> Service C
                   |
                   +--> Service D
```

This reduces coupling, simplifies orchestration logic, and allows new capabilities to be introduced without modifying existing workflows.

---

## 3. Every Service Is Both a Producer and Consumer

A service is never limited to a single role.

### 3.1 Student Service

Publishes:

- StudentCreated
- StudentUpdated
- StudentDeleted

Subscribes to:

- EnrollmentCreated
- AttendanceRecorded

### 3.2 Enrollment Service

Publishes:

- EnrollmentCreated
- EnrollmentCancelled

Subscribes to:

- StudentCreated
- CourseCreated

This allows services to participate in workflows while remaining independent.

---

## 4. Event-Driven Collaboration

Consider a new student registration.

Instead of:

```text
Student Orchestration Service
    -> Enrollment Service
        -> Timetable Service
            -> Notification Service
```

The workflow becomes:

```text
Student Service
    -> StudentCreated Event

Enrollment Service
    <- StudentCreated Event
    -> EnrollmentCreated Event

Timetable Service
    <- EnrollmentCreated Event
    -> TimetableGenerated Event

Notification Service
    <- TimetableGenerated Event
```

Each service only focuses on its own responsibility.

The workflow emerges naturally through event subscriptions.

---

## 5. Why This Approach?

### 5.1 Loose Coupling

Services do not need references to each other.

New services can be added without changing existing services.

### 5.2 Scalability

Multiple services can react to the same event.

A single event may trigger one consumer today and ten consumers tomorrow.

### 5.3 Extensibility

New functionality is added by subscribing to existing events.

No changes are required to the original publisher.

### 5.4 Testability

Each service can be tested independently.

Events become clear contracts between components.

### 5.5 Evolution Over Time

Systems naturally grow.

An event-driven architecture allows new capabilities to be introduced without creating large dependency chains.

---

## 6. Event Contracts

Events represent facts that have already happened.

Good examples:

```text
StudentCreated
StudentUpdated
CoursePublished
PaymentReceived
AttendanceRecorded
```

Events should describe completed actions, not requests.

Avoid:

```text
CreateStudent
GenerateInvoice
SendEmail
```

Prefer:

```text
StudentCreated
InvoiceGenerated
EmailSent
```

This keeps events focused on business outcomes rather than implementation details.

---

## 7. The Event Envelope

Events travel through the substrate inside a common envelope.

```csharp
public sealed class EventEnvelope<T>
{
    public Guid EventId { get; init; }
    public string EventName { get; init; }
    public DateTimeOffset OccurredDate { get; init; }

    public SecurityContext SecurityContext { get; init; }
    public RequestContext RequestContext { get; init; }

    public EnvelopeIntegrity Integrity { get; init; }

    public T Payload { get; init; }
}
```

The envelope provides:

- Event identity
- Correlation information
- Security context
- Request context
- Integrity validation
- Event payload

This allows all events to travel through the substrate using a consistent format.

---

## 8. Architectural Goal

The long-term goal is simple:

> Every service should be capable of emitting events, receiving events, or both.

Services communicate through events.

Services remain independently deployable and independently testable.

The Event Substrate provides the shared communication layer that allows the entire system to evolve without creating tight coupling between components.

---

## 9. Foundation Service Event Extensions

In a traditional implementation, a service exposes public methods that can be called directly by other services.

```text
Service A
    |
    +--> Service B
    |
    +--> Service C
```

Over time this creates tight coupling because services become aware of each other's contracts and implementation details.

Within the Event Substrate architecture, Foundation Services remain responsible for business operations, but can be extended with event receivers that react to events published through the substrate.

```text
Event Substrate
        |
        v
Student Foundation Service Receiver
        |
        v
Student Foundation Service
```

The receiver acts as an adapter between the Event Substrate and the Foundation Service. Its responsibility is to:

- Subscribe to events
- Validate incoming event data
- Map event payloads to service models
- Invoke the appropriate Foundation Service operation

The business logic remains inside the Foundation Service.

### 9.1 Internal Event Receivers

A recommended approach is for event receiver implementations to be internal to their owning service.

```csharp
internal sealed class StudentCreatedEventReceiver
{
}
```

Because the receiver is not publicly accessible:

- Other services cannot call it directly.
- The receiver can only be activated through the Event Substrate.
- Event processing remains consistent regardless of the publisher.
- Architectural boundaries are enforced by the compiler.

This helps prevent a common anti-pattern where developers bypass the event system and begin creating direct service-to-service dependencies.

### 9.2 Enforcing Communication Through the Substrate

The goal is not simply to receive events.

The goal is to ensure that services collaborate through the Event Substrate rather than through direct references to one another.

When event receivers are internal and Foundation Services only expose their intended public API, the Event Substrate becomes the single mechanism for event-driven collaboration.

This preserves loose coupling, improves maintainability, and allows new subscribers to be introduced without changing existing services.

### 9.3 Architectural Flow

```text
Controller
    |
    v
Orchestration Service
    |
    v
Foundation Service
    |
    v
Event Substrate
    |
    +--> Internal Event Receiver
              |
              v
       Foundation Service
```

This pattern allows Foundation Services to participate as both event emitters and event receivers while maintaining the architectural principles of The Standard.

---

## 10. Foundation Service Validation

Foundation Service validation still applies in the normal intent flow.

The intent flow is the standard application path where a caller intentionally asks the system to perform an operation.

```text
Controller
    |
    v
Orchestration Service
    |
    v
Foundation Service
```

In this flow, the Foundation Service validates the incoming model in the normal way.

For example:

- Required values are checked.
- Invalid values are rejected.
- CreatedBy/UpdatedBy and CreatedWhen/UpdatedWhen are the same for the add operation.
- CreatedBy/UpdatedBy and CreatedWhen/UpdatedWhen are different for the modify operation.
- CreatedWhen are recent for the add operation.
- UpdatedWhen are recent for the modify operation.


This remains the standard validation model for direct business operations.

### 10.1 Event Flow Validation

The event flow is different.

In the event flow, the service is not receiving a normal user intent. It is reacting to something that has already happened.

```text
Foundation Service A
    |
    v
Event Substrate
    |
    v
Foundation Service B
```

Because events represent completed facts, the receiving Foundation Service must take special care when validating the incoming event.

The receiver should still validate the event envelope and event payload, but it should not blindly treat the event in the same way as a normal user request.

The service needs to consider:

- Is the event envelope valid?
- Is the envelope integrity valid?
- Is the event payload valid?
- Has this event already been processed?
- Is this event still relevant?
- Is the receiving service allowed to react to this event?
- Should audit values come from the server (current time) or from the event envelope (time it occurred)?

This means the event receiver path may need validation rules that are different from the normal intent path.

### 10.2 Audit Field Decision

A specific design decision is required for audit fields.

Examples:

```text
CreatedWhen
UpdatedWhen
DeletedWhen
```

Or:

```text
CreatedAt
UpdatedAt
DeletedAt
```

The team must decide whether these values should represent:

1. The time the receiving service performed the action.
2. The time the original event occurred.

Both options are valid, but they communicate different meaning.

#### 10.3.1 Option 1: Server-Side Audit Values

The first option is to use server-side audit values.

In this approach, the receiving Foundation Service uses the `dateTimeBroker` to set or override the audit fields at the time the action is performed.

```text
Event Received
    |
    v
Foundation Service Receiver
    |
    v
dateTimeBroker.GetCurrentDateTimeOffset()
    |
    v
Set CreatedWhen / UpdatedWhen / DeletedWhen
```

This means the audit fields describe when the receiving service made the change.

This option is useful when the audit fields are intended to represent local persistence activity.

For example:

> This record was created in this service at this time.

Benefits:

- The receiving service remains the authority for its own audit fields.
- Audit values are generated consistently by the server.
- Consumers cannot tamper with persistence audit values.
- Precision and timezone handling are controlled by the server.
- The normal Foundation Service audit pattern remains consistent.

Trade-off:

- The audit value may not match the exact time the original event occurred.

### 10.3.2 Option 2: Event Envelope Audit Values

The second option is to use values from the event envelope.

In this approach, the receiving Foundation Service uses the event occurrence time from the envelope when setting audit fields.

```text
EventEnvelope.OccurredDate
    |
    v
Set CreatedWhen / UpdatedWhen / DeletedWhen
```

This means the audit fields describe when the original event happened.

This option is useful when the receiving service is building a projection or replica that should reflect the timeline of the source event.

For example:

> This record was created because an event occurred at this time.

Benefits:

- The receiving data reflects the original event timeline.
- Replayed events can preserve historical event dates.
- Projections can be rebuilt with the same audit timeline.
- Event-driven read models can more accurately represent source activity.

Trade-off:

- The receiving service is no longer using the local action time as the audit value.
- The service must trust the envelope date after integrity validation.
- The meaning of the audit fields changes from local persistence time to event occurrence time.
- Confusing timeline as events that happened after normal intent activities may have earlier audit values.

### 10.5 Recommended Clarity

Whichever option is chosen, the meaning of the audit fields must be clear.

If audit fields mean local persistence time, use server-side values from the `dateTimeBroker`.

If audit fields mean event occurrence time, use the event envelope values.

Avoid mixing both meanings for the same fields because it will make the data difficult to reason about later.

If both values are important, consider storing them separately.

For example:

```text
CreatedWhen        = when the record was created locally
SourceOccurredWhen = when the source event occurred
```

This keeps audit behavior explicit and avoids confusion between the intent flow and the event flow.

### 10.6 Validation Principle

The Foundation Service should continue to protect its own data.

The event receiver should not bypass validation simply because an event came through the substrate.

However, event validation should be designed with the understanding that the service is reacting to a completed event, not processing a normal user command.

The intent flow validates what the caller wants to do.

The event flow validates what the service is willing to accept and apply from an event.

