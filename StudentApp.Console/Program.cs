// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Extensions;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Services.Orchestrations.Students;

// ── DI setup ─────────────────────────────────────────
const string connectionString =
    "Server=(localdb)\\mssqllocaldb;Database=StudentAppPoc;Trusted_Connection=True;";

IServiceCollection services = new ServiceCollection();
services.AddStudentApp(connectionString);

IServiceProvider serviceProvider = services.BuildServiceProvider();

// ── Apply EF Core migrations ──────────────────────────
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IStorageBroker>();
    // Cast to DbContext to access Database
    if (db is DbContext dbContext)
        dbContext.Database.Migrate();
}

// ── Wire event receivers into LeVent ─────────────────
serviceProvider.RegisterEventReceivers();

// ── Demo: onboard a student ──────────────────────────
IStudentOrchestrationService orchestrationService =
    serviceProvider.GetRequiredService<IStudentOrchestrationService>();

var student = new Student
{
    Id = Guid.NewGuid(),
    FirstName = "Jane",
    LastName = "Doe",
    Email = "jane.doe@example.com",
    DateOfBirth = new DateOnly(1995, 6, 15),
    Status = "Pending"
};

Console.WriteLine("=== StudentApp Event Substrate POC ===");
Console.WriteLine($"Onboarding student: {student.FirstName} {student.LastName} ({student.Id})");
Console.WriteLine();

Student addedStudent =
    await orchestrationService.OnboardStudentAsync(student);

Console.WriteLine();
Console.WriteLine($"Student {addedStudent.Id} onboarded successfully.");
Console.WriteLine("=== Done ===");

