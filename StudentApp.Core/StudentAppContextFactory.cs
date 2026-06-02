// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore.Design;
using StudentApp.Core.Brokers.Storages;

namespace Glory2Him.Core
{
    internal class StudentAppContextFactory : IDesignTimeDbContextFactory<StorageBroker>
    {
        public StorageBroker CreateDbContext(string[] args)
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=StudentAppPoc;Trusted_Connection=True;";

            return new StorageBroker(connectionString);
        }
    }
}
