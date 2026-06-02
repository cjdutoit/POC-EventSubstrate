// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Notifications
{
    public interface INotificationService :
        IEventReceiver<StudentAddedEvent>,
        IEventReceiver<TimetableGeneratedEvent>
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
