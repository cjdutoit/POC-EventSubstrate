// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Xeptions;

namespace StudentApp.Security.Client.Models.Foundations.Users.Exceptions
{
    internal class ClaimNotFoundUserException : Xeption
    {
        public ClaimNotFoundUserException(string message)
            : base(message)
        { }
    }
}