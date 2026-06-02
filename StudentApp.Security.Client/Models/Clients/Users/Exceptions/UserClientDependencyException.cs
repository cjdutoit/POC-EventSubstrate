// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections;
using Xeptions;

namespace StudentApp.Security.Client.Models.Clients.Users.Exceptions
{
    public class UserClientDependencyException : Xeption
    {
        public UserClientDependencyException(string message, Xeption innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}