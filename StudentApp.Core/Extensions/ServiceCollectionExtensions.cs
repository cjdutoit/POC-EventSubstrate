// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using StudentApp.Core.Brokers.DateTimes;
using StudentApp.Core.Brokers.Emails;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Serializations;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Services.Foundations.Enrollments;
using StudentApp.Core.Services.Foundations.Notifications;
using StudentApp.Core.Services.Foundations.Students;
using StudentApp.Core.Services.Foundations.Timetables;
using StudentApp.Core.Services.Orchestrations.Students;
using StudentApp.Security.Client.Models.Clients;

namespace StudentApp.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStudentApp(
            this IServiceCollection services,
            string connectionString,
            SecurityConfigurations? securityConfigurations = null)
        {
            securityConfigurations ??= new SecurityConfigurations();

            // Brokers
            services.AddHttpContextAccessor();
            services.AddSingleton(securityConfigurations);
            services.AddScoped<ISecurityBroker, SecurityBroker>();
            services.AddSingleton<ILeVentClientRegistry, LeVentClientRegistry>();
            services.AddSingleton<IEventSubstrateBroker, EventSubstrateBroker>();
            services.AddSingleton<IEventEnvelopeFactory, EventEnvelopeFactory>();
            services.AddSingleton<ILoggingBroker, LoggingBroker>();
            services.AddSingleton<IEmailBroker, EmailBroker>();
            services.AddSingleton<IDateTimeBroker, DateTimeBroker>();
            services.AddSingleton<IJsonSerializationBroker, JsonSerializationBroker>();
            services.AddScoped<IStorageBroker>(_ => new StorageBroker(connectionString));

            // Foundation Services
            services.AddTransient<StudentService>();
            services.AddTransient<IStudentService>(sp => sp.GetRequiredService<StudentService>());
            services.AddTransient<EnrollmentService>();
            services.AddTransient<IEnrollmentService>(sp => sp.GetRequiredService<EnrollmentService>());
            services.AddTransient<TimetableService>();
            services.AddTransient<ITimetableService>(sp => sp.GetRequiredService<TimetableService>());
            services.AddTransient<NotificationService>();
            services.AddTransient<INotificationService>(sp => sp.GetRequiredService<NotificationService>());

            // Orchestration
            services.AddTransient<IStudentOrchestrationService, StudentOrchestrationService>();

            return services;
        }

        public static void RegisterEventReceivers(this IServiceProvider serviceProvider)
        {
            serviceProvider.RegisterStudentEventReceivers();
            serviceProvider.RegisterEnrollmentEventReceivers();
        }

        private static void RegisterStudentEventReceivers(this IServiceProvider serviceProvider)
        {
            serviceProvider.RegisterEventReceiver<StudentEnrolledEvent, StudentService>(
                StudentEventNames.StudentEnrolled);

            serviceProvider.RegisterEventReceiver<StudentEnrolledEvent, TimetableService>(
                StudentEventNames.StudentEnrolled);

            serviceProvider.RegisterEventReceiver<StudentAddedEvent, NotificationService>(
                StudentEventNames.StudentAdded);

            serviceProvider.RegisterEventReceiver<TimetableGeneratedEvent, NotificationService>(
                StudentEventNames.TimetableGenerated);
        }

        private static void RegisterEnrollmentEventReceivers(this IServiceProvider serviceProvider)
        {
            // Wire Enrollment-channel receivers here as handlers are introduced.
            // Example:
            // serviceProvider.RegisterEventReceiver<EnrollmentAddedEvent, SomeService>(
            //     EnrollmentEventNames.EnrollmentAdded);
        }
    }
}
