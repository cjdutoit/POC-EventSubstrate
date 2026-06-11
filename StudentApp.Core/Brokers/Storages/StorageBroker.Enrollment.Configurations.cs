// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore;
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

            model
                .Property(enrollment => enrollment.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(enrollment => enrollment.CreatedWhen)
                .IsRequired();

            model
                .Property(enrollment => enrollment.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property(enrollment => enrollment.UpdatedWhen)
                .IsRequired();

            model
                .Property(enrollment => enrollment.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            model
                .Property(enrollment => enrollment.DeletedBy)
                .HasMaxLength(255)
                .IsRequired(false);

            model
                .Property(enrollment => enrollment.DeletedWhen)
                .IsRequired(false);

            model
                .Property(enrollment => enrollment.DeletionReason)
                .IsRequired(false);
        }
    }
}
