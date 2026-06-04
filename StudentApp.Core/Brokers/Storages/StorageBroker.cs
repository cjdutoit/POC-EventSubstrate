// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using EFxceptions;
using Microsoft.EntityFrameworkCore;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.ProcessedEvents;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public partial class StorageBroker : EFxceptionsContext, IStorageBroker
    {
        private readonly string connectionString;

        public StorageBroker(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            AddConfigurations(modelBuilder);
        }

        private static void AddConfigurations(ModelBuilder modelBuilder)
        {
            AddStudentConfigurations(modelBuilder.Entity<Student>());
            AddEnrollmentConfigurations(modelBuilder.Entity<Enrollment>());
            AddProcessedEventConfigurations(modelBuilder.Entity<ProcessedEvent>());
        }

        private async ValueTask<T> InsertAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class
        {
            this.Entry(@object).State = EntityState.Added;
            await this.SaveChangesAsync(cancellationToken);
            return @object;
        }

        private async ValueTask<T> SelectByIdAsync<T>(object[] objectIds, CancellationToken cancellationToken = default)
            where T : class
        {
            return await this.Set<T>().FindAsync(objectIds, cancellationToken);
        }

        private IQueryable<T> SelectAll<T>() where T : class =>
            this.Set<T>().AsNoTracking();

        private async ValueTask<T> UpdateAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class
        {
            this.Entry(@object).State = EntityState.Modified;
            await this.SaveChangesAsync(cancellationToken);
            return @object;
        }

        private async ValueTask<T> DeleteAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class
        {
            this.Entry(@object).State = EntityState.Deleted;
            await this.SaveChangesAsync(cancellationToken);
            return @object;
        }
    }
}
