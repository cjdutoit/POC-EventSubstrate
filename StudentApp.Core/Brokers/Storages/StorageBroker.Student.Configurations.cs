// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

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
        }
    }
}
