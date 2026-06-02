// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System.Linq;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Students
{
    public partial class StudentServiceTests
    {
        [Fact]
        public void ShouldRetrieveAllStudents()
        {
            // given
            IQueryable<Student> randomStudents =
                CreateRandomStudents();

            IQueryable<Student> storageStudents = randomStudents;
            IQueryable<Student> expectedStudents = storageStudents;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAllStudents())
                    .Returns(storageStudents);

            // when
            IQueryable<Student> actualStudents =
                this.studentService.RetrieveAllStudents();

            // then
            actualStudents.Should().BeEquivalentTo(expectedStudents);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAllStudents(),
                    Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
