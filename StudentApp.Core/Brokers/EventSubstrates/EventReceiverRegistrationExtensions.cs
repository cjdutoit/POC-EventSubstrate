// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using LeVent.Clients;
using Microsoft.Extensions.DependencyInjection;
using StudentApp.Core.Models.Events;

namespace StudentApp.Core.Brokers.EventSubstrates
{
    internal static class EventReceiverRegistrationExtensions
    {
        /// <summary>
        /// Wires a registered <see cref="IEventReceiver{TEvent}"/> into the LeVent client
        /// for the given <paramref name="eventName"/>. Call this after the DI container is built.
        /// </summary>
        public static void RegisterEventReceiver<TEvent, TReceiver>(
            this IServiceProvider serviceProvider,
            string eventName)
            where TReceiver : IEventReceiver<TEvent>
        {
            ILeVentClientRegistry registry =
                serviceProvider.GetRequiredService<ILeVentClientRegistry>();

            ILeVentClient<EventEnvelope<TEvent>> client = registry.GetOrCreate<TEvent>();

            client.RegisterEventHandler(
                eventHandler: async (EventEnvelope<TEvent> envelope) =>
                {
                    using IServiceScope scope = serviceProvider.CreateScope();

                    TReceiver receiver =
                        scope.ServiceProvider.GetRequiredService<TReceiver>();

                    await receiver.ReceiveAsync(envelope);
                },
                eventName: eventName);
        }
    }
}
