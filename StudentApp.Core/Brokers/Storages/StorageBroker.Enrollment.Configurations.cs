// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        private static void AddEnrollmentConfigurations(EntityTypeBuilder<Enrollment> model)
        {
            model.HasKey(e => e.Id);

            model.Property(e => e.CourseCode)
                .IsRequired()
                .HasMaxLength(50);

            model.Property(e => e.Status)
                .HasMaxLength(50);
        }
    }
}
