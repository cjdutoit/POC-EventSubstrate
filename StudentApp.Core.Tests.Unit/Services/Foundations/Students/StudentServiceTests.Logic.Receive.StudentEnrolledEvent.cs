// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.ProcessedEvents;
using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Services.Foundations.Students;
using Xunit;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldReceiveStudentEnrolledEventAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            randomStudent.Status = string.Empty;

            StudentEnrolledEvent studentEnrolledEvent = new StudentEnrolledEvent
            {
                StudentId = randomStudent.Id
            };

            EventEnvelope<StudentEnrolledEvent> inputEnvelope =
                new EventEnvelope<StudentEnrolledEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentEnrolled,
                        Version = 1
                    },
                    SecurityContext = new SecurityContext
                    {
                        Username = "test-user"
                    },
                    RequestContext = new RequestContext(),
                    Integrity = new EnvelopeIntegrity(),
                    Content = studentEnrolledEvent
                };

            EventEnvelope<StudentModifiedEvent> expectedModifiedEnvelope =
                CreateStudentModifiedEnvelope(randomStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(randomStudent);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(randomStudent))
                .ReturnsAsync(randomStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.UpdateStudentAsync(
                    It.IsAny<Student>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(randomStudent);

            this.eventSubstrateBrokerMock.Setup(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<StudentModifiedEvent>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // when
            IEventReceiver<StudentEnrolledEvent> eventReceiver =
                this.studentService as IEventReceiver<StudentEnrolledEvent>;

            await eventReceiver.ReceiveAsync(inputEnvelope, CancellationToken.None);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectProcessedEventExistsAsync(
                    It.IsAny<Guid>(),
                    nameof(StudentService),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    randomStudent.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateStudentAsync(
                    It.IsAny<Student>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertProcessedEventAsync(
                    It.IsAny<ProcessedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<StudentModifiedEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains(StudentEventNames.StudentEnrolled) &&
                        msg.Contains(randomStudent.Id.ToString()))),
                Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldNotPublishEventWhenStudentDoesNotExistAsync()
        {
            // given
            Guid nonExistentStudentId = Guid.NewGuid();

            StudentEnrolledEvent studentEnrolledEvent = new StudentEnrolledEvent
            {
                StudentId = nonExistentStudentId
            };

            EventEnvelope<StudentEnrolledEvent> inputEnvelope =
                new EventEnvelope<StudentEnrolledEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentEnrolled,
                        Version = 1
                    },
                    SecurityContext = new SecurityContext
                    {
                        Username = "test-user"
                    },
                    RequestContext = new RequestContext(),
                    Integrity = new EnvelopeIntegrity(),
                    Content = studentEnrolledEvent
                };

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(
                    nonExistentStudentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Student)null);

            // when
            IEventReceiver<StudentEnrolledEvent> eventReceiver =
                this.studentService as IEventReceiver<StudentEnrolledEvent>;

            await eventReceiver.ReceiveAsync(inputEnvelope, CancellationToken.None);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.SelectProcessedEventExistsAsync(
                    It.IsAny<Guid>(),
                    nameof(StudentService),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(
                    nonExistentStudentId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<StudentModifiedEvent>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(It.IsAny<string>()),
                Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
