// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Linq.Expressions;
using Moq;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Services.Foundations.Enrollments;
using StudentApp.Core.Services.Foundations.Students;
using StudentApp.Core.Services.Orchestrations.Students;
using Tynamix.ObjectFiller;
using Xeptions;

namespace StudentApp.Core.Tests.Unit.Services.Orchestrations.Students
{
    public partial class StudentOrchestrationServiceTests
    {
        private readonly Mock<IStudentService> studentServiceMock;
        private readonly Mock<IEnrollmentService> enrollmentServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IStudentOrchestrationService studentOrchestrationService;

        public StudentOrchestrationServiceTests()
        {
            this.studentServiceMock = new Mock<IStudentService>();
            this.enrollmentServiceMock = new Mock<IEnrollmentService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.studentOrchestrationService = new StudentOrchestrationService(
                studentService: this.studentServiceMock.Object,
                enrollmentService: this.enrollmentServiceMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Student CreateRandomStudent() =>
            CreateStudentFiller().Create();

        private static Filler<Student> CreateStudentFiller()
        {
            string userId = new MnemonicString(1).GetValue();
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            var filler = new Filler<Student>();

            filler.Setup()
                .OnProperty(s => s.Id).Use(Guid.NewGuid())
                .OnProperty(s => s.DateOfBirth).Use(DateOnly.FromDateTime(DateTime.Now))
                .OnProperty(s => s.Status).Use(string.Empty)
                .OnProperty(s => s.CreatedBy).Use(userId)
                .OnProperty(s => s.UpdatedBy).Use(userId)
                .OnProperty(s => s.CreatedWhen).Use(dateTimeOffset)
                .OnProperty(s => s.UpdatedWhen).Use(dateTimeOffset)
                .OnProperty(s => s.IsDeleted).Use(false)
                .OnProperty(s => s.DeletedBy).Use((string?)null)
                .OnProperty(s => s.DeletedWhen).Use((DateTimeOffset?)null)
                .OnProperty(s => s.DeletionReason).Use((string?)null);

            return filler;
        }

        private static Enrollment CreateRandomEnrollment(Guid studentId)
        {
            string userId = new MnemonicString(1).GetValue();
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            var filler = new Filler<Enrollment>();

            filler.Setup()
                .OnProperty(e => e.Id).Use(Guid.NewGuid())
                .OnProperty(e => e.StudentId).Use(studentId)
                .OnProperty(e => e.CourseCode).Use("DEFAULT-101")
                .OnProperty(e => e.Status).Use("Active")
                .OnProperty(e => e.CreatedBy).Use(userId)
                .OnProperty(e => e.UpdatedBy).Use(userId)
                .OnProperty(e => e.CreatedWhen).Use(dateTimeOffset)
                .OnProperty(e => e.UpdatedWhen).Use(dateTimeOffset)
                .OnProperty(e => e.IsDeleted).Use(false)
                .OnProperty(e => e.DeletedBy).Use((string?)null)
                .OnProperty(e => e.DeletedWhen).Use((DateTimeOffset?)null)
                .OnProperty(e => e.DeletionReason).Use((string?)null)
                .OnType<DateTimeOffset>().Use(dateTimeOffset);

            return filler.Create();
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException) =>
            actualException => actualException.Message == expectedException.Message
                && actualException.InnerException.Message == expectedException.InnerException.Message;
    }
}
