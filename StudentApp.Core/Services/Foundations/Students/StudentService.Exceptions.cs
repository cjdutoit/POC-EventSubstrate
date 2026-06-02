// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using EFxceptions.Models.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Foundations.Students.Exceptions;
using Xeptions;

namespace StudentApp.Core.Services.Foundations.Students
{
    public sealed partial class StudentService
    {
        private delegate ValueTask<Student> ReturningStudentFunction();
        private delegate IQueryable<Student> ReturningStudentsFunction();

        private async ValueTask<Student> TryCatch(ReturningStudentFunction returningStudentFunction)
        {
            try
            {
                return await returningStudentFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Student operation timed out, contact support.",
                        innerException: new TimeoutException(),
                        data: operationCanceledException.Data);

                throw CreateAndLogDependencyException(failedStudentStorageException);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (NullStudentException nullStudentException)
            {
                throw CreateAndLogValidationException(nullStudentException);
            }
            catch (InvalidStudentException invalidStudentException)
            {
                throw CreateAndLogValidationException(invalidStudentException);
            }
            catch (NotFoundStudentException notFoundStudentException)
            {
                throw CreateAndLogValidationException(notFoundStudentException);
            }
            catch (SqlException sqlException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw CreateAndLogCriticalDependencyException(failedStudentStorageException);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExistsStudentException =
                    new AlreadyExistsStudentException(
                        message: "Student with the same id already exists.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data);

                throw CreateAndLogDependencyValidationException(alreadyExistsStudentException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var lockedStudentException =
                    new LockedStudentException(
                        message: "Locked student record, please try again later.",
                        innerException: dbUpdateConcurrencyException,
                        data: dbUpdateConcurrencyException.Data);

                throw CreateAndLogDependencyValidationException(lockedStudentException);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: dbUpdateException,
                        data: dbUpdateException.Data);

                throw CreateAndLogDependencyException(failedStudentStorageException);
            }
            catch (Exception exception)
            {
                var failedStudentServiceException =
                    new FailedStudentServiceException(
                        message: "Failed student service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedStudentServiceException);
            }
        }

        private IQueryable<Student> TryCatch(ReturningStudentsFunction returningStudentsFunction)
        {
            try
            {
                return returningStudentsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedStudentStorageException =
                    new FailedStudentStorageException(
                        message: "Failed student storage error occurred, contact support.",
                        innerException: sqlException,
                        data: sqlException.Data);

                throw CreateAndLogCriticalDependencyException(failedStudentStorageException);
            }
            catch (Exception exception)
            {
                var failedStudentServiceException =
                    new FailedStudentServiceException(
                        message: "Failed student service error occurred, contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw CreateAndLogServiceException(failedStudentServiceException);
            }
        }

        private StudentValidationException CreateAndLogValidationException(Xeption exception)
        {
            var studentValidationException =
                new StudentValidationException(
                    message: "Student validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(studentValidationException);

            return studentValidationException;
        }

        private StudentDependencyValidationException CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var studentDependencyValidationException =
                new StudentDependencyValidationException(
                    message: "Student dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(studentDependencyValidationException);

            return studentDependencyValidationException;
        }

        private StudentDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var studentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(studentDependencyException);

            return studentDependencyException;
        }

        private StudentDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var studentDependencyException =
                new StudentDependencyException(
                    message: "Student dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(studentDependencyException);

            return studentDependencyException;
        }

        private StudentServiceException CreateAndLogServiceException(Xeption exception)
        {
            var studentServiceException =
                new StudentServiceException(
                    message: "Student service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(studentServiceException);

            return studentServiceException;
        }
    }
}
