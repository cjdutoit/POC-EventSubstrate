// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Core.Models.Foundations.Notifications.Exceptions
{
    public class InvalidNotificationException : Xeption
    {
        public InvalidNotificationException(string message)
            : base(message)
        {
        }
    }
}
