// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Brokers.Emails
{
    public sealed class EmailBroker : IEmailBroker
    {
        public ValueTask SendWelcomeEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask SendTimetableEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}
