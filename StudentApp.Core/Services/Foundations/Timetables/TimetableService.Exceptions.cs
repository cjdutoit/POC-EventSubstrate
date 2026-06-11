// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Timetables.Exceptions;
using Xeptions;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public sealed partial class TimetableService
    {
        private delegate ValueTask ReturningNothingFunction();

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var failedTimetableServiceException =
                    new FailedTimetableServiceException(
                        message: "Timetable operation timed out, contact support.",
                        innerException: new TimeoutException(),
                        data: operationCanceledException.Data);

                throw CreateAndLogServiceException(failedTimetableServiceException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidTimetableException invalidTimetableException)
            {
                throw CreateAndLogValidationException(invalidTimetableException);
            }
            catch (Exception exception)
            {
                var failedTimetableServiceException =
                    new FailedTimetableServiceException(
                        message: "Failed timetable service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedTimetableServiceException);
            }
        }


        private async ValueTask TryCatch<T>(
            Func<ValueTask> operation,
            EventEnvelope<T> envelope,
            CancellationToken cancellationToken)
        {
            try
            {
                await operation();
            }
            catch (Exception exception)
            {
                this.loggingBroker.LogError(exception);

                var failureEvent = new ReactionFailedEvent
                {
                    OriginalEventId = envelope.Metadata.EventId,
                    OriginalEventType = envelope.Metadata.EventType,
                    OriginalEventPayload = await this.jsonSerializationBroker.SerializeAsync(envelope.Content),
                    ReceiverServiceName = nameof(TimetableService),
                    ErrorMessage = exception.Message,
                    ErrorStackTrace = exception.StackTrace,
                    FailedAt = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync()
                };

                EventEnvelope<ReactionFailedEvent> failureEnvelope =
                    new EventEnvelope<ReactionFailedEvent>
                    {
                        Metadata = new EventMetadata
                        {
                            EventId = Guid.NewGuid(),
                            EventType = StudentEventNames.ReactionFailed,
                            Version = 1,
                            CausationId = envelope.Metadata.EventId.ToString(),
                            ParentCorrelationId = envelope.Metadata.ParentCorrelationId,
                        },
                        SecurityContext = envelope.SecurityContext,
                        RequestContext = envelope.RequestContext,
                        Content = failureEvent
                    };

                await this.eventSubstrateBroker.EmitAsync(
                    failureEnvelope,
                    cancellationToken);
            }
        }

        private static void ValidateEnvelopeIsNotNull(EventEnvelope<StudentEnrolledEvent> envelope)
        {
            if (envelope is null)
                throw new ArgumentNullException(nameof(envelope), "Event envelope cannot be null.");
        }

        private static void ValidateEnvelopeContent(EventEnvelope<StudentEnrolledEvent> envelope)
        {
            if (envelope.Content is null)
                throw new ArgumentNullException(nameof(envelope.Content), "Event content cannot be null.");

            if (envelope.Content.StudentId == Guid.Empty)
                throw new ArgumentException("StudentId cannot be empty.", nameof(envelope.Content.StudentId));
        }

        private TimetableValidationException CreateAndLogValidationException(Xeption exception)
        {
            var timetableValidationException =
                new TimetableValidationException(
                    message: "Timetable validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(timetableValidationException);

            return timetableValidationException;
        }

        private TimetableServiceException CreateAndLogServiceException(Xeption exception)
        {
            var timetableServiceException =
                new TimetableServiceException(
                    message: "Timetable service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(timetableServiceException);

            return timetableServiceException;
        }
    }
}
