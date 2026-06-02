// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Brokers.Emails
{
    public interface IEmailBroker
    {
        ValueTask SendWelcomeEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);

        ValueTask SendTimetableEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }
}
