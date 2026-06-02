// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Users.Exceptions
{
    internal class InvalidArgumentUserException : Xeption
    {
        public InvalidArgumentUserException(string message)
            : base(message)
        { }
    }
}