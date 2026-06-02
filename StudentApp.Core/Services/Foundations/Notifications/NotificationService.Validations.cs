// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Notifications.Exceptions;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public sealed partial class NotificationService
    {
        private static void ValidateSendWelcomeEmailArguments(Guid studentId, string email)
        {
            Validate(
                (Rule: IsInvalid(studentId), Parameter: nameof(studentId)),
                (Rule: IsInvalid(email), Parameter: nameof(email)));
        }

        private static void ValidateSendTimetableEmailArguments(Guid studentId)
        {
            Validate(
                (Rule: IsInvalid(studentId), Parameter: nameof(studentId)));
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
            var invalidNotificationException =
                new InvalidNotificationException(
                    message: "Invalid notification. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidNotificationException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidNotificationException.ThrowIfContainsErrors();
        }
    }
}
