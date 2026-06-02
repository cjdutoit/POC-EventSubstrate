// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events.StudentEvents;

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public interface ITimetableService : IEventReceiver<StudentEnrolledEvent>
    {
        ValueTask GenerateTimetableAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }
}
