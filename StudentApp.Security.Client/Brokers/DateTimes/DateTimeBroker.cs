// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading.Tasks;

namespace StudentApp.Security.Client.Brokers.DateTimes
{
    internal class DateTimeBroker : IDateTimeBroker
    {
        public ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync() =>
            ValueTask.FromResult(DateTimeOffset.UtcNow);
    }
}