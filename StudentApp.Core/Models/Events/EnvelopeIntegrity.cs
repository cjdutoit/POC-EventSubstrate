// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Models.Events
{
    public sealed class EnvelopeIntegrity
    {
        public string Algorithm { get; init; } = "HMACSHA256";
        public string Signature { get; init; } = string.Empty;
        public DateTimeOffset SignedDate { get; init; }
    }
}
