// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker
    {
        private static void AddStudentConfigurations(EntityTypeBuilder<Student> model)
        {
            model.HasKey(s => s.Id);

            model.Property(s => s.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            model.Property(s => s.LastName)
                .IsRequired()
                .HasMaxLength(100);

            model.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(200);

            model.Property(s => s.Status)
                .HasMaxLength(50);

            model.Property(s => s.CreatedBy).HasMaxLength(255).IsRequired();
            model.Property(s => s.CreatedWhen).IsRequired();
            model.Property(s => s.UpdatedBy).HasMaxLength(255).IsRequired();
            model.Property(s => s.UpdatedWhen).IsRequired();
            model.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
            model.Property(s => s.DeletedBy).HasMaxLength(255).IsRequired(false);
            model.Property(s => s.DeletedWhen).IsRequired(false);
            model.Property(s => s.DeletionReason).IsRequired(false);
        }
    }
}
