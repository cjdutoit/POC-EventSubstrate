// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Notifications.Exceptions;
using Xeptions;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public sealed partial class NotificationService
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
                var failedNotificationServiceException =
                    new FailedNotificationServiceException(
                        message: "Notification operation timed out, contact support.",
                        innerException: new TimeoutException(),
                        data: operationCanceledException.Data);

                throw CreateAndLogServiceException(failedNotificationServiceException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidNotificationException invalidNotificationException)
            {
                throw CreateAndLogValidationException(invalidNotificationException);
            }
            catch (Exception exception)
            {
                var failedNotificationServiceException =
                    new FailedNotificationServiceException(
                        message: "Failed notification service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedNotificationServiceException);
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
                    ReceiverServiceName = nameof(NotificationService),
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

        private static void ValidateStudentAddedEnvelope(EventEnvelope<StudentAddedEvent> envelope)
        {
            if (envelope is null)
                throw new ArgumentNullException(nameof(envelope), "Event envelope cannot be null.");

            if (envelope.Content is null)
                throw new ArgumentNullException(nameof(envelope.Content), "Event content cannot be null.");

            if (envelope.Content.StudentId == Guid.Empty)
                throw new ArgumentException("StudentId cannot be empty.", nameof(envelope.Content.StudentId));
        }

        private static void ValidateTimetableGeneratedEnvelope(EventEnvelope<TimetableGeneratedEvent> envelope)
        {
            if (envelope is null)
                throw new ArgumentNullException(nameof(envelope), "Event envelope cannot be null.");

            if (envelope.Content is null)
                throw new ArgumentNullException(nameof(envelope.Content), "Event content cannot be null.");

            if (envelope.Content.StudentId == Guid.Empty)
                throw new ArgumentException("StudentId cannot be empty.", nameof(envelope.Content.StudentId));
        }

        private NotificationValidationException CreateAndLogValidationException(Xeption exception)
        {
            var notificationValidationException =
                new NotificationValidationException(
                    message: "Notification validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(notificationValidationException);

            return notificationValidationException;
        }

        private NotificationServiceException CreateAndLogServiceException(Xeption exception)
        {
            var notificationServiceException =
                new NotificationServiceException(
                    message: "Notification service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(notificationServiceException);

            return notificationServiceException;
        }
    }
}
