// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Moq;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveStudentByIdAsync()
        {
            // given
            Student randomStudent = CreateRandomStudent();
            Guid inputStudentId = randomStudent.Id;
            Student storageStudent = randomStudent;
            Student expectedStudent = storageStudent.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.SelectStudentByIdAsync(inputStudentId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storageStudent);

            // when
            Student actualStudent =
                await this.studentService.RetrieveStudentByIdAsync(inputStudentId);

            // then
            actualStudent.Should().BeEquivalentTo(expectedStudent);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectStudentByIdAsync(inputStudentId, It.IsAny<CancellationToken>()),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
