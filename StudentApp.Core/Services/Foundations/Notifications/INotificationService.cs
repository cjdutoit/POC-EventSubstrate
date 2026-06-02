// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public interface INotificationService
    {
        ValueTask SendWelcomeEmailAsync(
            Guid studentId,
            string email,
            CancellationToken cancellationToken = default);

        ValueTask SendTimetableEmailAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }
}
