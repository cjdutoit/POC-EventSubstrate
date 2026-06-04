// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;
using Moq;
using StudentApp.Core.Brokers.DateTimes;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.EnrollmentEvents;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Enrollments;
using StudentApp.Core.Services.Foundations.Enrollments;
using Tynamix.ObjectFiller;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Enrollments
{
    public partial class EnrollmentServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IEventSubstrateBroker> eventSubstrateBrokerMock;
        private readonly Mock<IEventEnvelopeFactory> envelopeFactoryMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<ISecurityBroker> securityBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly IEnrollmentService enrollmentService;

        public EnrollmentServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.eventSubstrateBrokerMock = new Mock<IEventSubstrateBroker>();
            this.envelopeFactoryMock = new Mock<IEventEnvelopeFactory>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.securityBrokerMock = new Mock<ISecurityBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.dateTimeBrokerMock.Setup(x => x.GetCurrentDateTimeOffset())
                .Returns(DateTimeOffset.UtcNow);

            this.enrollmentService = new EnrollmentService(
                storageBroker: this.storageBrokerMock.Object,
                eventSubstrateBroker: this.eventSubstrateBrokerMock.Object,
                envelopeFactory: this.envelopeFactoryMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object);
        }

        private static Enrollment CreateRandomEnrollment() =>
            CreateEnrollmentFiller().Create();

        private static IQueryable<Enrollment> CreateRandomEnrollments() =>
            CreateEnrollmentFiller().Create(GetRandomNumber()).AsQueryable();

        private static Filler<Enrollment> CreateEnrollmentFiller()
        {
            var filler = new Filler<Enrollment>();

            filler.Setup()
                .OnProperty(e => e.Id).Use(Guid.NewGuid())
                .OnProperty(e => e.StudentId).Use(Guid.NewGuid())
                .OnProperty(e => e.CourseCode).Use(new MnemonicString(1, 3, 10).GetValue())
                .OnProperty(e => e.EnrolledAt).Use(DateTimeOffset.UtcNow)
                .OnProperty(e => e.Status).Use("Active");

            return filler;
        }

        private static EventEnvelope<EnrollmentAddedEvent> CreateEnrollmentAddedEnvelope(
            Enrollment enrollment) =>
            new EventEnvelope<EnrollmentAddedEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = EnrollmentEventNames.EnrollmentAdded,
                    Version = 1,
                },
                Content = new EnrollmentAddedEvent
                {
                    EnrollmentId = enrollment.Id,
                    StudentId = enrollment.StudentId,
                    CourseCode = enrollment.CourseCode,
                    EnrolledAt = enrollment.EnrolledAt,
                    Status = enrollment.Status
                }
            };

        private static EventEnvelope<StudentEnrolledEvent> CreateStudentEnrolledEnvelope(
            Enrollment enrollment) =>
            new EventEnvelope<StudentEnrolledEvent>
            {
                Metadata = new EventMetadata
                {
                    EventId = Guid.NewGuid(),
                    EventType = StudentEventNames.StudentEnrolled,
                    Version = 1,
                },
                Content = new StudentEnrolledEvent
                {
                    StudentId = enrollment.StudentId,
                    Status = enrollment.Status
                }
            };

        private static SqlException CreateSqlException() =>
            (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 9).GetValue();

        private static Guid CreateRandomGuid() => Guid.NewGuid();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException) =>
            actualException => actualException.Message == expectedException.Message
                && actualException.InnerException.Message == expectedException.InnerException.Message;
    }
}
