// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Timetables.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Timetables
{
    public partial class TimetableServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGenerateIfUnexpectedErrorOccursAsync()
        {
            // given
            Guid randomStudentId = CreateRandomGuid();
            var serviceException = new Exception();

            var failedTimetableServiceException =
                new FailedTimetableServiceException(
                    message: "Failed timetable service error occurred, contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedTimetableServiceException =
                new TimetableServiceException(
                    message: "Timetable service error occurred, contact support.",
                    innerException: failedTimetableServiceException);

            this.envelopeFactoryMock
                .Setup(factory => factory.CreateAsync(
                    It.IsAny<TimetableGeneratedEvent>(),
                    It.IsAny<string>(),
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(serviceException);

            // when
            ValueTask generateTask =
                this.timetableService.GenerateTimetableAsync(
                    randomStudentId,
                    CancellationToken.None);

            TimetableServiceException actualException =
                await Assert.ThrowsAsync<TimetableServiceException>(
                    generateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTimetableServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedTimetableServiceException))),
                Times.Once);

            this.envelopeFactoryMock.Verify(factory =>
                factory.CreateAsync(
                    It.IsAny<TimetableGeneratedEvent>(),
                    StudentEventNames.TimetableGenerated,
                    It.IsAny<SecurityContext>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
