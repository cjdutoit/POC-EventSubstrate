// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using LeVent.Clients;
using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public interface ILeVentClientRegistry
    {
        ILeVentClient<EventEnvelope<TEvent>> GetOrCreate<TEvent>();
    }
}
