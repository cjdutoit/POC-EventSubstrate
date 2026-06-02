// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using StudentApp.Security.Client.Brokers.DateTimes;
using StudentApp.Security.Client.Clients.Audits;
using StudentApp.Security.Client.Clients.Users;
using StudentApp.Security.Client.Services.Foundations.Audits;
using StudentApp.Security.Client.Services.Foundations.Users;
using StudentApp.Security.Client.Services.Orchestrations.Audits;

namespace StudentApp.Security.Client.Clients
{
    public class SecurityClient : ISecurityClient
    {
        public SecurityClient()
        {
            IServiceProvider serviceProvider = RegisterServices();
            InitializeClients(serviceProvider);
        }

        public IUserClient Users { get; private set; }
        public IAuditClient Audits { get; private set; }

        private void InitializeClients(IServiceProvider serviceProvider)
        {
            Users = serviceProvider.GetRequiredService<IUserClient>();
            Audits = serviceProvider.GetRequiredService<IAuditClient>();
        }

        private static IServiceProvider RegisterServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<IDateTimeBroker, DateTimeBroker>()
                .AddTransient<IAuditService, AuditService>()
                .AddTransient<IUserService, UserService>()
                .AddTransient<IAuditOrchestrationService, AuditOrchestrationService>()
                .AddTransient<IUserClient, UserClient>()
                .AddTransient<IAuditClient, AuditClient>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
