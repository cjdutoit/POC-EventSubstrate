// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Notifications.Exceptions
{
    public class NotificationValidationException : Xeption
    {
        public NotificationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        {
        }
    }
}
