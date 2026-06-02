// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Core.Models.Foundations.Notifications.Exceptions
{
    public class FailedNotificationServiceException : Xeption
    {
        public FailedNotificationServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        {
        }
    }
}
