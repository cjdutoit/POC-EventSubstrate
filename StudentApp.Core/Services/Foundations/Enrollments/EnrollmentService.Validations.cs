// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;

namespace StudentApp.Core.Services.Foundations.Enrollments
{
    public sealed partial class EnrollmentService
    {
        private static void ValidateEnrollmentOnAdd(Enrollment enrollment)
        {
            ValidateEnrollmentIsNotNull(enrollment);

            Validate(
                (Rule: IsInvalid(enrollment.Id), Parameter: nameof(Enrollment.Id)),
                (Rule: IsInvalid(enrollment.StudentId), Parameter: nameof(Enrollment.StudentId)),
                (Rule: IsInvalid(enrollment.CourseCode), Parameter: nameof(Enrollment.CourseCode)));
        }

        private static void ValidateEnrollmentOnModify(Enrollment enrollment)
        {
            ValidateEnrollmentIsNotNull(enrollment);

            Validate(
                (Rule: IsInvalid(enrollment.Id), Parameter: nameof(Enrollment.Id)),
                (Rule: IsInvalid(enrollment.StudentId), Parameter: nameof(Enrollment.StudentId)),
                (Rule: IsInvalid(enrollment.CourseCode), Parameter: nameof(Enrollment.CourseCode)));
        }

        private static void ValidateEnrollmentId(Guid enrollmentId) =>
            Validate((Rule: IsInvalid(enrollmentId), Parameter: nameof(Enrollment.Id)));

        private static void ValidateStorageEnrollment(Enrollment maybeEnrollment, Guid enrollmentId)
        {
            if (maybeEnrollment is null)
            {
                throw new NotFoundEnrollmentException(
                    message: $"Could not find enrollment with id: {enrollmentId}.");
            }
        }

        private static void ValidateEnrollmentIsNotNull(Enrollment enrollment)
        {
            if (enrollment is null)
            {
                throw new NullEnrollmentException(
                    message: "Enrollment is null.");
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidEnrollmentException =
                new InvalidEnrollmentException(
                    message: "Invalid enrollment. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidEnrollmentException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidEnrollmentException.ThrowIfContainsErrors();
        }
    }
}
