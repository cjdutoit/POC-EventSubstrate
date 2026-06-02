// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldRemoveStudentByIdAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Guid inputStudentId = randomStudent.Id;
            Student storageStudent = randomStudent;
            Student deletedStudent = storageStudent;
            Student expectedStudent = deletedStudent.DeepClone();

            EventEnvelope<StudentRemovedEvent> envelope =
                CreateStudentRemovedEnvelope(deletedStudent);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudentId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageStudent);

            this.securityBrokerMock.Setup(broker =>
                broker.ApplyRemoveAuditValuesAsync(storageStudent))
                    .ReturnsAsync(storageStudent);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentSecurityContextAsync())
                    .ReturnsAsync(new SecurityContext());

            this.storageBrokerMock.Setup(broker =>
                broker.DeleteStudentAsync(storageStudent, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(deletedStudent);

            this.envelopeFactoryMock.Setup(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentRemovedEvent>(),
                    StudentEventNames.StudentRemoved,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // when
            Student actualStudent =
                await this.studentService.RemoveStudentByIdAsync(inputStudentId);

            // then
            actualStudent.Should().BeEquivalentTo(expectedStudent);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudentId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteStudentAsync(storageStudent, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.ApplyRemoveAuditValuesAsync(storageStudent),
                    Times.Once);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentSecurityContextAsync(),
                    Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<StudentRemovedEvent>(),
                    StudentEventNames.StudentRemoved,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.Verify(broker =>
                broker.EmitAsync(It.IsAny<EventEnvelope<StudentRemovedEvent>>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformation(
                    It.Is<string>(msg => msg.Contains(inputStudentId.ToString()))),
                Times.AtLeastOnce);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
        }
    }
}
