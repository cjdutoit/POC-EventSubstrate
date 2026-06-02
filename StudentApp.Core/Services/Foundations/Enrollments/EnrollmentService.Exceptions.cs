// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using EFxceptions.Models.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;
using Xeptions;

namespace StudentApp.Core.Services.Foundations.Enrollments
{
    public sealed partial class EnrollmentService
    {
        private delegate ValueTask<Enrollment> ReturningEnrollmentFunction();
        private delegate IQueryable<Enrollment> ReturningEnrollmentsFunction();

        private async ValueTask<Enrollment> TryCatch(ReturningEnrollmentFunction returningEnrollmentFunction)
        {
            try
            {
                return await returningEnrollmentFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var failedEnrollmentStorageException =
                    new FailedEnrollmentStorageException(
                        message: "Enrollment operation timed out, contact support.",
                        innerException: new TimeoutException(),
                        data: operationCanceledException.Data);

                throw CreateAndLogDependencyException(failedEnrollmentStorageException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (NullEnrollmentException nullEnrollmentException)
            {
                throw CreateAndLogValidationException(nullEnrollmentException);
            }
            catch (InvalidEnrollmentException invalidEnrollmentException)
            {
                throw CreateAndLogValidationException(invalidEnrollmentException);
            }
            catch (NotFoundEnrollmentException notFoundEnrollmentException)
            {
                throw CreateAndLogValidationException(notFoundEnrollmentException);
            }
            catch (SqlException sqlException)
            {
                var failedEnrollmentStorageException =
                    new FailedEnrollmentStorageException(
                        message: "Failed enrollment storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw CreateAndLogCriticalDependencyException(failedEnrollmentStorageException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsEnrollmentException =
                    new AlreadyExistsEnrollmentException(
                        message: "Enrollment with the same id already exists.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw CreateAndLogDependencyValidationException(alreadyExistsEnrollmentException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedEnrollmentException =
                    new LockedEnrollmentException(
                        message: "Locked enrollment record, please try again later.",
                        innerException: dbUpdateConcurrencyException,
                        data: dbUpdateConcurrencyException.Data);

                throw CreateAndLogDependencyValidationException(lockedEnrollmentException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedEnrollmentStorageException =
                    new FailedEnrollmentStorageException(
                        message: "Failed enrollment storage error occurred, contact support.",
                        innerException: dbUpdateException,
                        data: dbUpdateException.Data);

                throw CreateAndLogDependencyException(failedEnrollmentStorageException);
            }
            catch (Exception exception)
            {
                var failedEnrollmentServiceException =
                    new FailedEnrollmentServiceException(
                        message: "Failed enrollment service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedEnrollmentServiceException);
            }
        }

        private IQueryable<Enrollment> TryCatch(ReturningEnrollmentsFunction returningEnrollmentsFunction)
        {
            try
            {
                return returningEnrollmentsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedEnrollmentStorageException =
                    new FailedEnrollmentStorageException(
                        message: "Failed enrollment storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw CreateAndLogCriticalDependencyException(failedEnrollmentStorageException);
            }
            catch (Exception exception)
            {
                var failedEnrollmentServiceException =
                    new FailedEnrollmentServiceException(
                        message: "Failed enrollment service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedEnrollmentServiceException);
            }
        }

        private EnrollmentValidationException CreateAndLogValidationException(Xeption exception)
        {
            var enrollmentValidationException =
                new EnrollmentValidationException(
                    message: "Enrollment validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(enrollmentValidationException);

            return enrollmentValidationException;
        }

        private EnrollmentDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var enrollmentDependencyValidationException =
                new EnrollmentDependencyValidationException(
                    message: "Enrollment dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(enrollmentDependencyValidationException);

            return enrollmentDependencyValidationException;
        }

        private EnrollmentDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var enrollmentDependencyException =
                new EnrollmentDependencyException(
                    message: "Enrollment dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(enrollmentDependencyException);

            return enrollmentDependencyException;
        }

        private EnrollmentDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var enrollmentDependencyException =
                new EnrollmentDependencyException(
                    message: "Enrollment dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(enrollmentDependencyException);

            return enrollmentDependencyException;
        }

        private EnrollmentServiceException CreateAndLogServiceException(Xeption exception)
        {
            var enrollmentServiceException =
                new EnrollmentServiceException(
                    message: "Enrollment service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(enrollmentServiceException);

            return enrollmentServiceException;
        }
    }
}
