// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Users.Exceptions
{
    internal class UserDependencyException : Xeption
    {
        public UserDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}