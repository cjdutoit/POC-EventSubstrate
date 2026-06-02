// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Foundations.Students.Exceptions;
using StudentApp.Core.Models.Orchestrations.Students.Exceptions;
using Xeptions;

namespace StudentApp.Core.Services.Orchestrations.Students
{
    public sealed partial class StudentOrchestrationService
    {
        private delegate ValueTask<Student> ReturningStudentFunction();

        private async ValueTask<Student> TryCatch(ReturningStudentFunction returningStudentFunction)
        {
            try
            {
                return await returningStudentFunction();
            }
            catch (NullStudentOrchestrationException nullStudentOrchestrationException)
            {
                throw CreateAndLogValidationException(nullStudentOrchestrationException);
            }
            catch (InvalidStudentOrchestrationException invalidStudentOrchestrationException)
            {
                throw CreateAndLogValidationException(invalidStudentOrchestrationException);
            }
            catch (StudentValidationException studentValidationException)
            {
                throw CreateAndLogDependencyValidationException(studentValidationException);
            }
            catch (StudentDependencyValidationException studentDependencyValidationException)
            {
                throw CreateAndLogDependencyValidationException(studentDependencyValidationException);
            }
            catch (StudentDependencyException studentDependencyException)
            {
                throw CreateAndLogDependencyException(studentDependencyException);
            }
            catch (StudentServiceException studentServiceException)
            {
                throw CreateAndLogServiceException(studentServiceException);
            }
            catch (EnrollmentValidationException enrollmentValidationException)
            {
                throw CreateAndLogDependencyValidationException(enrollmentValidationException);
            }
            catch (EnrollmentDependencyValidationException enrollmentDependencyValidationException)
            {
                throw CreateAndLogDependencyValidationException(enrollmentDependencyValidationException);
            }
            catch (EnrollmentDependencyException enrollmentDependencyException)
            {
                throw CreateAndLogDependencyException(enrollmentDependencyException);
            }
            catch (EnrollmentServiceException enrollmentServiceException)
            {
                throw CreateAndLogServiceException(enrollmentServiceException);
            }
            catch (Exception exception)
            {
                var failedStudentOrchestrationServiceException =
                    new StudentOrchestrationServiceException(
                        message: "Failed student orchestration service error occurred, contact support.",
                        innerException: exception as Xeption ?? new Xeption(exception.Message, exception));

                throw CreateAndLogServiceException(failedStudentOrchestrationServiceException);
            }
        }

        private StudentOrchestrationValidationException CreateAndLogValidationException(Xeption exception)
        {
            var studentOrchestrationValidationException =
                new StudentOrchestrationValidationException(
                    message: "Student orchestration validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(studentOrchestrationValidationException);

            return studentOrchestrationValidationException;
        }

        private StudentOrchestrationDependencyValidationException CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var studentOrchestrationDependencyValidationException =
                new StudentOrchestrationDependencyValidationException(
                    message: "Student orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            this.loggingBroker.LogError(studentOrchestrationDependencyValidationException);

            return studentOrchestrationDependencyValidationException;
        }

        private StudentOrchestrationDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var studentOrchestrationDependencyException =
                new StudentOrchestrationDependencyException(
                    message: "Student orchestration dependency error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(studentOrchestrationDependencyException);

            return studentOrchestrationDependencyException;
        }

        private StudentOrchestrationServiceException CreateAndLogServiceException(Xeption exception)
        {
            var studentOrchestrationServiceException =
                new StudentOrchestrationServiceException(
                    message: "Student orchestration service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(studentOrchestrationServiceException);

            return studentOrchestrationServiceException;
        }
    }
}
