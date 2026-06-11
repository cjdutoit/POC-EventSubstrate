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
            this.dateTimeBrokerMock.Setup(x => x.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(() => DateTimeOffset.UtcNow);

            this.enrollmentService = new EnrollmentService(
                storageBroker: this.storageBrokerMock.Object,
                eventSubstrateBroker: this.eventSubstrateBrokerMock.Object,
                envelopeFactory: this.envelopeFactoryMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                securityBroker: this.securityBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object);
        }

        private static Enrollment CreateRandomEnrollment() =>
            CreateEnrollmentFiller(DateTimeOffset.UtcNow, GetRandomString()).Create();

        private static Enrollment CreateRandomModifyEnrollment(DateTimeOffset dateTimeOffset, string userId)
        {
            Enrollment enrollment = CreateEnrollmentFiller(dateTimeOffset, userId).Create();
            enrollment.CreatedWhen = enrollment.CreatedWhen.AddDays(GetRandomNegativeNumber());

            return enrollment;
        }

        private static IQueryable<Enrollment> CreateRandomEnrollments() =>
            CreateEnrollmentFiller(DateTimeOffset.UtcNow, GetRandomString()).Create(GetRandomNumber()).AsQueryable();

        private static Filler<Enrollment> CreateEnrollmentFiller(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? GetRandomString() : userId;
            var filler = new Filler<Enrollment>();

            filler.Setup()
                .OnProperty(e => e.Id).Use(Guid.NewGuid())
                .OnProperty(e => e.StudentId).Use(Guid.NewGuid())
                .OnProperty(e => e.CourseCode).Use(new MnemonicString(1, 3, 10).GetValue())
                .OnProperty(e => e.EnrolledAt).Use(dateTimeOffset)
                .OnProperty(e => e.Status).Use("Active")
                .OnProperty(e => e.CreatedBy).Use(userId)
                .OnProperty(e => e.UpdatedBy).Use(userId)
                .OnProperty(e => e.CreatedWhen).Use(dateTimeOffset)
                .OnProperty(e => e.UpdatedWhen).Use(dateTimeOffset)
                .OnProperty(e => e.IsDeleted).Use(false)
                .OnProperty(e => e.DeletedBy).Use((string?)null)
                .OnProperty(e => e.DeletedWhen).Use((DateTimeOffset?)null)
                .OnProperty(e => e.DeletionReason).Use((string?)null);

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

        public static TheoryData<int> MinutesBeforeOrAfter()
        {
            int randomTimeInFuture = GetRandomNumber();
            int randomTimeInPast = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomTimeInFuture,
                randomTimeInPast
            };
        }

        private static SqlException CreateSqlException() =>
            (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 9).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Guid CreateRandomGuid() => Guid.NewGuid();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static Expression<Func<Exception, bool>> SameExceptionAs(
            Exception expectedException) =>
            actualException => actualException.Message == expectedException.Message
                && actualException.InnerException.Message == expectedException.InnerException.Message;
    }
}
