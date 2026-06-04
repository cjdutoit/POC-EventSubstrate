// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.DateTimes;
using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.EnrollmentEvents;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Enrollments;

namespace StudentApp.Core.Services.Foundations.Enrollments
{
    public sealed partial class EnrollmentService : IEnrollmentService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IEventSubstrateBroker eventSubstrateBroker;
        private readonly IEventEnvelopeFactory envelopeFactory;
        private readonly ILoggingBroker loggingBroker;
        private readonly ISecurityBroker securityBroker;
        private readonly IDateTimeBroker dateTimeBroker;

        public EnrollmentService(
            IStorageBroker storageBroker,
            IEventSubstrateBroker eventSubstrateBroker,
            IEventEnvelopeFactory envelopeFactory,
            ILoggingBroker loggingBroker,
            ISecurityBroker securityBroker,
            IDateTimeBroker dateTimeBroker)
        {
            this.storageBroker = storageBroker;
            this.eventSubstrateBroker = eventSubstrateBroker;
            this.envelopeFactory = envelopeFactory;
            this.loggingBroker = loggingBroker;
            this.securityBroker = securityBroker;
            this.dateTimeBroker = dateTimeBroker;
        }

        public ValueTask<Enrollment> AddEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateEnrollmentOnAdd(enrollment);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                return await DoAddEnrollmentAsync(enrollment, securityContext, cancellationToken);
            });

        public IQueryable<Enrollment> RetrieveAllEnrollments() =>
            TryCatch(() => this.storageBroker.SelectAllEnrollments());

        public ValueTask<Enrollment> RetrieveEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateEnrollmentId(enrollmentId);

                Enrollment maybeEnrollment =
                    await this.storageBroker.SelectEnrollmentByIdAsync(
                        enrollmentId,
                        cancellationToken);

                ValidateStorageEnrollment(maybeEnrollment, enrollmentId);

                return maybeEnrollment;
            });

        public ValueTask<Enrollment> ModifyEnrollmentAsync(
            Enrollment enrollment,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateEnrollmentOnModify(enrollment);

                Enrollment maybeEnrollment =
                    await this.storageBroker.SelectEnrollmentByIdAsync(
                        enrollment.Id,
                        cancellationToken);

                ValidateStorageEnrollment(maybeEnrollment, enrollment.Id);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                return await DoModifyEnrollmentAsync(enrollment, securityContext, cancellationToken);
            });

        public ValueTask<Enrollment> RemoveEnrollmentByIdAsync(
            Guid enrollmentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateEnrollmentId(enrollmentId);

                Enrollment maybeEnrollment =
                    await this.storageBroker.SelectEnrollmentByIdAsync(
                        enrollmentId,
                        cancellationToken);

                ValidateStorageEnrollment(maybeEnrollment, enrollmentId);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                return await DoRemoveEnrollmentAsync(maybeEnrollment, securityContext, cancellationToken);
            });

        private async ValueTask<Enrollment> DoAddEnrollmentAsync(
            Enrollment enrollment,
            SecurityContext securityContext,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Adding enrollment for student {enrollment.StudentId}");

            enrollment.EnrolledAt = this.dateTimeBroker.GetCurrentDateTimeOffset();
            enrollment.Status = "Active";

            Enrollment addedEnrollment =
                await this.storageBroker.InsertEnrollmentAsync(
                    enrollment,
                    cancellationToken);

            EventEnvelope<EnrollmentAddedEvent> outboundEnrollmentEnvelope =
                await this.envelopeFactory.CreateAsync(
                    new EnrollmentAddedEvent
                    {
                        EnrollmentId = addedEnrollment.Id,
                        StudentId = addedEnrollment.StudentId,
                        CourseCode = addedEnrollment.CourseCode,
                        EnrolledAt = addedEnrollment.EnrolledAt,
                        Status = addedEnrollment.Status
                    },
                    EnrollmentEventNames.EnrollmentAdded,
                    securityContext,
                    cancellationToken);

            EventEnvelope<StudentEnrolledEvent> outboundStudentEnvelope =
                await this.envelopeFactory.CreateAsync(
                    new StudentEnrolledEvent
                    {
                        StudentId = addedEnrollment.StudentId,
                        Status = addedEnrollment.Status
                    },
                    StudentEventNames.StudentEnrolled,
                    securityContext,
                    cancellationToken);

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Added enrollment {addedEnrollment.Id} for student {addedEnrollment.StudentId}");

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Emitting {outboundEnrollmentEnvelope.Metadata.EventType} event to " +
                    $"Substrate for enrollment {addedEnrollment.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnrollmentEnvelope,
                cancellationToken);

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Emitting {outboundStudentEnvelope.Metadata.EventType} event to " +
                    $"Substrate for student {addedEnrollment.StudentId}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundStudentEnvelope,
                cancellationToken);

            return addedEnrollment;
        }

        private async ValueTask<Enrollment> DoModifyEnrollmentAsync(
            Enrollment enrollment,
            SecurityContext securityContext,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Modifying enrollment {enrollment.Id} for student {enrollment.StudentId}");

            Enrollment updatedEnrollment =
                await this.storageBroker.UpdateEnrollmentAsync(
                    enrollment,
                    cancellationToken);

            EventEnvelope<EnrollmentModifiedEvent> outboundEnvelope =
                await this.envelopeFactory.CreateAsync(
                    new EnrollmentModifiedEvent
                    {
                        EnrollmentId = updatedEnrollment.Id,
                        StudentId = updatedEnrollment.StudentId,
                        CourseCode = updatedEnrollment.CourseCode,
                        EnrolledAt = updatedEnrollment.EnrolledAt,
                        Status = updatedEnrollment.Status
                    },
                    EnrollmentEventNames.EnrollmentModified,
                    securityContext,
                    cancellationToken);

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Modified enrollment {updatedEnrollment.Id} for student {updatedEnrollment.StudentId}");

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for enrollment {updatedEnrollment.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);

            return updatedEnrollment;
        }

        private async ValueTask<Enrollment> DoRemoveEnrollmentAsync(
            Enrollment enrollment,
            SecurityContext securityContext,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Removing enrollment {enrollment.Id} for student {enrollment.StudentId}");

            Enrollment deletedEnrollment =
                await this.storageBroker.DeleteEnrollmentAsync(
                    enrollment,
                    cancellationToken);

            EventEnvelope<EnrollmentRemovedEvent> outboundEnvelope =
                await this.envelopeFactory.CreateAsync(
                    new EnrollmentRemovedEvent
                    {
                        EnrollmentId = deletedEnrollment.Id,
                        StudentId = deletedEnrollment.StudentId,
                        CourseCode = deletedEnrollment.CourseCode,
                        EnrolledAt = deletedEnrollment.EnrolledAt,
                        Status = deletedEnrollment.Status
                    },
                    EnrollmentEventNames.EnrollmentRemoved,
                    securityContext,
                    cancellationToken);

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Removed enrollment {deletedEnrollment.Id} for student {deletedEnrollment.StudentId}");

            this.loggingBroker.LogInformation(
                $"[EnrollmentService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for enrollment {deletedEnrollment.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);

            return deletedEnrollment;
        }
    }
}
