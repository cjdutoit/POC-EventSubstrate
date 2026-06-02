// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

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
