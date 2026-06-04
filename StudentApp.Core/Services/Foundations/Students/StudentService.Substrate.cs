// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.ProcessedEvents;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Services.Foundations.Students
{
    public sealed partial class StudentService :
        IEventReceiver<StudentEnrolledEvent>
    {

        ValueTask IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken) =>
        TryCatch(
            async () => await HandleStudentEnrolledAsync(envelope, cancellationToken),
            envelope,
            cancellationToken);

        private async ValueTask HandleStudentEnrolledAsync(
            EventEnvelope<StudentEnrolledEvent> envelope,
            CancellationToken cancellationToken)
        {
            ValidateEnvelopeIsNotNull(envelope);
            ValidateEnvelopeContent(envelope);

            bool alreadyProcessed = await this.storageBroker.SelectProcessedEventExistsAsync(
                envelope.Metadata.EventId,
                nameof(StudentService),
                cancellationToken);

            if (alreadyProcessed)
                return;

            this.loggingBroker.LogInformation(
                $"[Substrate] Relaying {StudentEventNames.StudentEnrolled} to StudentService for student {envelope.Content.StudentId}");

            Student maybeStudent =
                await this.storageBroker.SelectStudentByIdAsync(
                    envelope.Content.StudentId,
                    cancellationToken);

            if (maybeStudent is null)
                return;

            maybeStudent.Status = "Enrolled";

            // We need to reflect who made the change and when
            // Design decision to use the OriginatedAt or the current time.
            // maybeStudent.UpdatedBy = envelope.SecurityContext.Username ?? envelope.SecurityContext.ClientId;
            // maybeStudent.UpdatedWhen = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            EventEnvelope<StudentModifiedEvent> enrolledEnvelope =
                new EventEnvelope<StudentModifiedEvent>
                {
                    Metadata = envelope.Metadata,
                    SecurityContext = envelope.SecurityContext,
                    RequestContext = envelope.RequestContext,
                    Integrity = envelope.Integrity,
                    Content = new StudentModifiedEvent
                    {
                        StudentId = maybeStudent.Id,
                        FirstName = maybeStudent.FirstName,
                        LastName = maybeStudent.LastName,
                        DateOfBirth = maybeStudent.DateOfBirth,
                        Email = maybeStudent.Email,
                        Status = maybeStudent.Status
                    }
                };

            await DoModifyStudentAsync(maybeStudent, enrolledEnvelope, cancellationToken);

            await this.storageBroker.InsertProcessedEventAsync(
                new ProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = envelope.Metadata.EventId,
                    ReceiverName = nameof(StudentService),
                    ProcessedAt = this.dateTimeBroker.GetCurrentDateTimeOffset()
                },
                cancellationToken);
        }
    }
}
