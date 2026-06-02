// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events
{
    /// <summary>
    /// Identifies the mechanism by which the caller was authenticated.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// The authentication type could not be determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A human user authenticated interactively, for example via cookie or OpenID Connect.
        /// </summary>
        User = 1,

        /// <summary>
        /// A non-human caller authenticated using client credentials, for example a background job or AI worker.
        /// </summary>
        Machine = 2,

        /// <summary>
        /// A caller is acting on behalf of another subject using delegated access.
        /// </summary>
        Delegated = 3,

        /// <summary>
        /// An internal system process authenticated without a human or external client principal.
        /// </summary>
        System = 4
    }
}
