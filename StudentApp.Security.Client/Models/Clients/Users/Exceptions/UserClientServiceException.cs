// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace StudentApp.Security.Client.Models.Clients.Users.Exceptions
{
    public class UserClientServiceException : Xeption
    {
        public UserClientServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}