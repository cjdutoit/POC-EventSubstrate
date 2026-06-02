// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Notifications.Exceptions
{
    public class NotificationServiceException : Xeption
    {
        public NotificationServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
