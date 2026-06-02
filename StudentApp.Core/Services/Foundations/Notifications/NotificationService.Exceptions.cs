// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

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
