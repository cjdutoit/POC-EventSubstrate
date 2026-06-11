// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Enrollments.Exceptions;

namespace StudentApp.Core.Services.Foundations.Enrollments
{
    public sealed partial class EnrollmentService
    {
        private async ValueTask ValidateEnrollmentOnAddAsync(Enrollment enrollment, string currentUserId)
        {
            ValidateEnrollmentIsNotNull(enrollment);

            Validate(
                (Rule: IsInvalid(enrollment.Id), Parameter: nameof(Enrollment.Id)),
                (Rule: IsInvalid(enrollment.StudentId), Parameter: nameof(Enrollment.StudentId)),
                (Rule: IsInvalid(enrollment.CourseCode), Parameter: nameof(Enrollment.CourseCode)),
                (Rule: IsInvalid(enrollment.CreatedBy), Parameter: nameof(Enrollment.CreatedBy)),
                (Rule: IsInvalid(enrollment.UpdatedBy), Parameter: nameof(Enrollment.UpdatedBy)),
                (Rule: IsInvalid(enrollment.CreatedWhen), Parameter: nameof(Enrollment.CreatedWhen)),
                (Rule: IsInvalid(enrollment.UpdatedWhen), Parameter: nameof(Enrollment.UpdatedWhen)),

                (Rule: IsGreaterThan(enrollment.CreatedBy, 255),
                    Parameter: nameof(Enrollment.CreatedBy)),

                (Rule: IsGreaterThan(enrollment.UpdatedBy, 255),
                    Parameter: nameof(Enrollment.UpdatedBy)),

                (Rule: IsNotSame(
                        firstDate: enrollment.UpdatedWhen,
                        secondDate: enrollment.CreatedWhen,
                        secondDateName: nameof(Enrollment.CreatedWhen)),
                    Parameter: nameof(Enrollment.UpdatedWhen)),

                (Rule: IsNotSame(
                        first: currentUserId,
                        second: enrollment.CreatedBy),
                    Parameter: nameof(Enrollment.CreatedBy)),

                (Rule: IsNotSame(
                        first: enrollment.UpdatedBy,
                        second: enrollment.CreatedBy,
                        secondName: nameof(Enrollment.CreatedBy)),
                    Parameter: nameof(Enrollment.UpdatedBy)),

                (Rule: await IsNotRecentAsync(enrollment.CreatedWhen),
                    Parameter: nameof(Enrollment.CreatedWhen)));
        }

        private async ValueTask ValidateEnrollmentOnModifyAsync(Enrollment enrollment, string currentUserId)
        {
            ValidateEnrollmentIsNotNull(enrollment);

            Validate(
                (Rule: IsInvalid(enrollment.Id), Parameter: nameof(Enrollment.Id)),
                (Rule: IsInvalid(enrollment.StudentId), Parameter: nameof(Enrollment.StudentId)),
                (Rule: IsInvalid(enrollment.CourseCode), Parameter: nameof(Enrollment.CourseCode)),
                (Rule: IsInvalid(enrollment.CreatedBy), Parameter: nameof(Enrollment.CreatedBy)),
                (Rule: IsInvalid(enrollment.UpdatedBy), Parameter: nameof(Enrollment.UpdatedBy)),
                (Rule: IsInvalid(enrollment.CreatedWhen), Parameter: nameof(Enrollment.CreatedWhen)),
                (Rule: IsInvalid(enrollment.UpdatedWhen), Parameter: nameof(Enrollment.UpdatedWhen)),

                (Rule: IsGreaterThan(enrollment.CreatedBy, 255),
                    Parameter: nameof(Enrollment.CreatedBy)),

                (Rule: IsGreaterThan(enrollment.UpdatedBy, 255),
                    Parameter: nameof(Enrollment.UpdatedBy)),

                (Rule: IsNotSame(
                        first: currentUserId,
                        second: enrollment.UpdatedBy),
                    Parameter: nameof(Enrollment.UpdatedBy)),

                (Rule: IsSame(
                        firstDate: enrollment.UpdatedWhen,
                        secondDate: enrollment.CreatedWhen,
                        secondDateName: nameof(Enrollment.CreatedWhen)),
                    Parameter: nameof(Enrollment.UpdatedWhen)),

                (Rule: await IsNotRecentAsync(enrollment.UpdatedWhen),
                    Parameter: nameof(Enrollment.UpdatedWhen)));
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

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsGreaterThan(string text, int maxLength) => new
        {
            Condition = !string.IsNullOrWhiteSpace(text) && text.Length > maxLength,
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private static dynamic IsNotSame(string first, string second) => new
        {
            Condition = first != second,
            Message = $"Expected value to be '{first}' but found '{second}'."
        };

        private static dynamic IsNotSame(string first, string second, string secondName) => new
        {
            Condition = first != second,
            Message = $"Text is not the same as {secondName}"
        };

        private async ValueTask<dynamic> IsNotRecentAsync(DateTimeOffset date)
        {
            if (date == default)
                return new { Condition = false, Message = string.Empty };

            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();
            DateTimeOffset startDate = currentDateTime.AddSeconds(-90);
            DateTimeOffset endDate = currentDateTime;
            bool isNotRecent = date < startDate || date > endDate;

            return new
            {
                Condition = isNotRecent,
                Message = $"Date is not recent. Expected a value between {startDate} and {endDate} but found {date}"
            };
        }

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
