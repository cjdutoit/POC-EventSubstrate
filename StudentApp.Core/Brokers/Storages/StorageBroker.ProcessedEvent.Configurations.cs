// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApp.Core.Models.Foundations.ProcessedEvents;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        private static void AddProcessedEventConfigurations(EntityTypeBuilder<ProcessedEvent> model)
        {
            model.HasKey(pe => pe.Id);

            model.HasIndex(pe => new { pe.EventId, pe.ReceiverName })
                .IsUnique();

            model.Property(pe => pe.ReceiverName)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}
