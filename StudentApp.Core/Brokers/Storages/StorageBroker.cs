// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using EFxceptions;
using Microsoft.EntityFrameworkCore;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Brokers.Storages
{
    public sealed class StorageBroker : EFxceptionsContext, IStorageBroker
    {
        private readonly string connectionString;

        public StorageBroker(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(s => s.Status)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CourseCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(50);
            });
        }

        public async ValueTask<Student> InsertStudentAsync(
            Student student,
            CancellationToken cancellationToken = default)
        {
            this.Entry(student).State = EntityState.Added;
            await this.SaveChangesAsync(cancellationToken);

            return student;
        }

        public IQueryable<Student> SelectAllStudents() =>
            this.Students.AsNoTracking();

        public async ValueTask<Student> SelectStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            return await this.Students.FindAsync(
                new object[] { studentId },
                cancellationToken);
        }

        public async ValueTask<Student> UpdateStudentAsync(
            Student student,
            CancellationToken cancellationToken = default)
        {
            this.Entry(student).State = EntityState.Modified;
            await this.SaveChangesAsync(cancellationToken);

            return student;
        }

        public async ValueTask<Student> DeleteStudentAsync(
            Student student,
            CancellationToken cancellationToken = default)
        {
            this.Entry(student).State = EntityState.Deleted;
            await this.SaveChangesAsync(cancellationToken);

            return student;
        }

        public async ValueTask<Enrollment> InsertEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default)
        {
            this.Entry(enrollment).State = EntityState.Added;
            await this.SaveChangesAsync(cancellationToken);

            return enrollment;
        }

        public IQueryable<Enrollment> SelectAllEnrollments() =>
            this.Enrollments.AsNoTracking();

        public async ValueTask<Enrollment> SelectEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default)
        {
            return await this.Enrollments.FindAsync(
                new object[] { enrollmentId },
                cancellationToken);
        }

        public async ValueTask<Enrollment> UpdateEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default)
        {
            this.Entry(enrollment).State = EntityState.Modified;
            await this.SaveChangesAsync(cancellationToken);

            return enrollment;
        }

        public async ValueTask<Enrollment> DeleteEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default)
        {
            this.Entry(enrollment).State = EntityState.Deleted;
            await this.SaveChangesAsync(cancellationToken);

            return enrollment;
        }
    }
}
