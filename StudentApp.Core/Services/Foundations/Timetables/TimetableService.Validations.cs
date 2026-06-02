// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Timetables.Exceptions;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public sealed partial class TimetableService
    {
        private static void ValidateGenerateTimetableArguments(Guid studentId)
        {
            Validate(
                (Rule: IsInvalid(studentId), Parameter: nameof(studentId)));
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidTimetableException =
                new InvalidTimetableException(
                    message: "Invalid timetable. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidTimetableException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidTimetableException.ThrowIfContainsErrors();
        }
    }
}
