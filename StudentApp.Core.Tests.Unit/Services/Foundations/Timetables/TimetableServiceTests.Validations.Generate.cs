// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StudentApp.Core.Models.Foundations.Timetables.Exceptions;

namespace StudentApp.Core.Tests.Unit.Services.Foundations.Timetables
{
    public partial class TimetableServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGenerateIfStudentIdIsEmptyAsync()
        {
            // given
            Guid emptyStudentId = Guid.Empty;

            var invalidTimetableException =
                new InvalidTimetableException(
                    message: "Invalid timetable. Please correct the errors and try again.");

            invalidTimetableException.UpsertDataList(
                key: "studentId",
                value: "Id is required");

            var expectedTimetableValidationException =
                new TimetableValidationException(
                    message: "Timetable validation error occurred, fix the errors and try again.",
                    innerException: invalidTimetableException);

            // when
            ValueTask generateTask =
                this.timetableService.GenerateTimetableAsync(
                    emptyStudentId,
                    CancellationToken.None);

            TimetableValidationException actualException =
                await Assert.ThrowsAsync<TimetableValidationException>(
                    generateTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedTimetableValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedTimetableValidationException))),
                Times.Once);

            this.envelopeFactoryMock.VerifyNoOtherCalls();
            this.eventSubstrateBrokerMock.VerifyNoOtherCalls();
        }
    }
}
