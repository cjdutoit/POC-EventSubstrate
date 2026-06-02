// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Collections.Concurrent;
using LeVent.Clients;
using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    public sealed class LeVentClientRegistry : ILeVentClientRegistry
    {
        private readonly ConcurrentDictionary<Type, object> clients = new();

        public ILeVentClient<EventEnvelope<TEvent>> GetOrCreate<TEvent>()
        {
            return (ILeVentClient<EventEnvelope<TEvent>>)this.clients.GetOrAdd(
                typeof(TEvent),
                _ => new LeVentClient<EventEnvelope<TEvent>>());
        }
    }
}
