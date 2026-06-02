// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.EnrollmentEvents;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Enrollments
{
    public partial class EnrollmentServiceTests
    {
        [Fact]
        public async Task ShouldAddEnrollmentAsync()
        {
            // given
            Enrollment randomEnrollment = CreateRandomEnrollment();
            Enrollment inputEnrollment = randomEnrollment;
            Enrollment storageEnrollment = inputEnrollment;
            Enrollment expectedEnrollment = storageEnrollment.DeepClone();

            EventEnvelope<EnrollmentAddedEvent> enrollmentEnvelope =
                CreateEnrollmentAddedEnvelope(inputEnrollment);

            EventEnvelope<StudentEnrolledEvent> studentEnrolledEnvelope =
                CreateStudentEnrolledEnvelope(inputEnrollment);

            var securityContext = new SecurityContext();

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(securityContext);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertEnrollmentAsync(inputEnrollment, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageEnrollment);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.Is<EnrollmentAddedEvent>(e => e.EnrollmentId == inputEnrollment.Id),
                    EnrollmentEventNames.EnrollmentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(enrollmentEnvelope);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.Is<StudentEnrolledEvent>(e => e.StudentId == inputEnrollment.StudentId),
                    StudentEventNames.StudentEnrolled,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(studentEnrolledEnvelope);

            // when
            Enrollment actualEnrollment =
                await this.enrollmentService.AddEnrollmentAsync(inputEnrollment);

            // then
            actualEnrollment.Should().BeEquivalentTo(expectedEnrollment,
                options => options.Excluding(e => e.EnrolledAt));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Adding enrollment for student {inputEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    $"[EnrollmentService] Added enrollment {inputEnrollment.Id} for student {inputEnrollment.StudentId}"),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains("Emitting") &&
                        msg.Contains(EnrollmentEventNames.EnrollmentAdded) &&
                        msg.Contains(inputEnrollment.Id.ToString()))),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg =>
                        msg.Contains("Emitting") &&
                        msg.Contains(StudentEventNames.StudentEnrolled) &&
                        msg.Contains(inputEnrollment.StudentId.ToString()))),
                        Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertEnrollmentAsync(inputEnrollment, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.Is<EnrollmentAddedEvent>(e => e.EnrollmentId == inputEnrollment.Id),
                    EnrollmentEventNames.EnrollmentAdded,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.Is<StudentEnrolledEvent>(e => e.StudentId == inputEnrollment.StudentId),
                    StudentEventNames.StudentEnrolled,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<EnrollmentAddedEvent>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(
                    It.IsAny<EventEnvelope<StudentEnrolledEvent>>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
