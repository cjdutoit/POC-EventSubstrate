// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Users.Exceptions
{
    internal class UserServiceException : Xeption
    {
        public UserServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}