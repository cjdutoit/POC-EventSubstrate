// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

namespace StudentApp.Core.Services.Foundations.Timetables
{
    public interface ITimetableService
    {
        ValueTask GenerateTimetableAsync(
            Guid studentId,
            CancellationToken cancellationToken = default);
    }
}
