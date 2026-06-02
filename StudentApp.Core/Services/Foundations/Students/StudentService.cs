// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Brokers.EventSubstrates;
using StudentApp.Core.Brokers.Loggings;
using StudentApp.Core.Brokers.Securities;
using StudentApp.Core.Brokers.Storages;
using StudentApp.Core.Models.Events;
using StudentApp.Core.Models.Events.StudentEvents;
using StudentApp.Core.Models.Foundations.Students;

namespace StudentApp.Core.Services.Foundations.Students
{
    public sealed partial class StudentService : IStudentService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IEventSubstrateBroker eventSubstrateBroker;
        private readonly IEventEnvelopeFactory envelopeFactory;
        private readonly ILoggingBroker loggingBroker;
        private readonly ISecurityBroker securityBroker;

        public StudentService(
            IStorageBroker storageBroker,
            IEventSubstrateBroker eventSubstrateBroker,
            IEventEnvelopeFactory envelopeFactory,
            ILoggingBroker loggingBroker,
            ISecurityBroker securityBroker)
        {
            this.storageBroker = storageBroker;
            this.eventSubstrateBroker = eventSubstrateBroker;
            this.envelopeFactory = envelopeFactory;
            this.loggingBroker = loggingBroker;
            this.securityBroker = securityBroker;
        }

        public ValueTask<Student> AddStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Validates the student object before any processing
                // Validates audit fields are correctly set
                ValidateStudentOnAdd(student);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<StudentAddedEvent> envelope =
                    await this.envelopeFactory.CreateAsync(
                        new StudentAddedEvent
                        {
                            StudentId = student.Id,
                            FirstName = student.FirstName,
                            LastName = student.LastName,
                            DateOfBirth = student.DateOfBirth,
                            Email = student.Email,
                            Status = student.Status
                        },
                        StudentEventNames.StudentAdded,
                        securityContext,
                        cancellationToken);

                return await DoAddStudentAsync(student, envelope, cancellationToken);
            });

        public IQueryable<Student> RetrieveAllStudents() =>
            TryCatch(() => this.storageBroker.SelectAllStudents());

        public ValueTask<Student> RetrieveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateStudentId(studentId);

                Student maybeStudent =
                    await this.storageBroker.SelectStudentByIdAsync(
                        studentId,
                        cancellationToken);

                ValidateStorageStudent(maybeStudent, studentId);

                return maybeStudent;
            });

        public ValueTask<Student> ModifyStudentAsync(
            Student student,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Validates the student object before any processing
                // Validates audit fields are correctly set
                ValidateStudentOnModify(student);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<StudentModifiedEvent> envelope =
                    await this.envelopeFactory.CreateAsync(
                        new StudentModifiedEvent
                        {
                            StudentId = student.Id,
                            FirstName = student.FirstName,
                            LastName = student.LastName,
                            DateOfBirth = student.DateOfBirth,
                            Email = student.Email,
                            Status = student.Status
                        },
                        StudentEventNames.StudentModified,
                        securityContext,
                        cancellationToken);

                return await DoModifyStudentAsync(student, envelope, cancellationToken);
            });

        public ValueTask<Student> RemoveStudentByIdAsync(
            Guid studentId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Validates the student object before any processing
                // Validates audit fields are correctly set
                ValidateStudentId(studentId);

                Student maybeStudent =
                    await this.storageBroker.SelectStudentByIdAsync(
                        studentId,
                        cancellationToken);

                ValidateStorageStudent(maybeStudent, studentId);
                maybeStudent = await this.securityBroker.ApplyRemoveAuditValuesAsync(maybeStudent);
                SecurityContext securityContext = await this.securityBroker.GetCurrentSecurityContextAsync();

                EventEnvelope<StudentRemovedEvent> envelope =
                    await this.envelopeFactory.CreateAsync(
                        new StudentRemovedEvent
                        {
                            StudentId = maybeStudent.Id,
                            FirstName = maybeStudent.FirstName,
                            LastName = maybeStudent.LastName,
                            DateOfBirth = maybeStudent.DateOfBirth,
                            Email = maybeStudent.Email,
                            Status = maybeStudent.Status
                        },
                        StudentEventNames.StudentRemoved,
                        securityContext,
                        cancellationToken);

                return await DoRemoveStudentAsync(maybeStudent, envelope, cancellationToken);
            });

        private async ValueTask<Student> DoAddStudentAsync(
            Student student,
            EventEnvelope<StudentAddedEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation($"[StudentService] Adding student {student.Id}");
            student = await this.securityBroker.ApplyAddAuditValuesAsync(student);

            Student addedStudent =
                await this.storageBroker.InsertStudentAsync(
                    student,
                    cancellationToken);

            EventEnvelope<StudentAddedEvent> outboundEnvelope =
                new EventEnvelope<StudentAddedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentAdded,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = new StudentAddedEvent
                    {
                        StudentId = addedStudent.Id,
                        FirstName = addedStudent.FirstName,
                        LastName = addedStudent.LastName,
                        DateOfBirth = addedStudent.DateOfBirth,
                        Email = addedStudent.Email,
                        Status = addedStudent.Status
                    }
                };

            this.loggingBroker.LogInformation(
                $"[StudentService] Added student {addedStudent.Id}");

            this.loggingBroker.LogInformation(
                $"[StudentService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for student {addedStudent.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);

            return addedStudent;
        }

        private async ValueTask<Student> DoModifyStudentAsync(
            Student student,
            EventEnvelope<StudentModifiedEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[StudentService] Modifying student {student.Id}");

            Student maybeStudent =
                await this.storageBroker.SelectStudentByIdAsync(
                    student.Id,
                    cancellationToken);

            ValidateStorageStudent(maybeStudent, student.Id);

            Student updatedStudent =
                await this.storageBroker.UpdateStudentAsync(
                    student,
                    cancellationToken);

            EventEnvelope<StudentModifiedEvent> outboundEnvelope =
                new EventEnvelope<StudentModifiedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentModified,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = new StudentModifiedEvent
                    {
                        StudentId = updatedStudent.Id,
                        FirstName = updatedStudent.FirstName,
                        LastName = updatedStudent.LastName,
                        DateOfBirth = updatedStudent.DateOfBirth,
                        Email = updatedStudent.Email,
                        Status = updatedStudent.Status
                    }
                };

            this.loggingBroker.LogInformation(
                $"[StudentService] Modified student {updatedStudent.Id}");

            this.loggingBroker.LogInformation(
                $"[StudentService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for student {updatedStudent.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);

            return updatedStudent;
        }

        private async ValueTask<Student> DoRemoveStudentAsync(
            Student student,
            EventEnvelope<StudentRemovedEvent> inboundEnvelope,
            CancellationToken cancellationToken)
        {
            this.loggingBroker.LogInformation(
                $"[StudentService] Removing student {student.Id}");


            Student deletedStudent =
                await this.storageBroker.DeleteStudentAsync(
                    student,
                    cancellationToken);

            EventEnvelope<StudentRemovedEvent> outboundEnvelope =
                new EventEnvelope<StudentRemovedEvent>
                {
                    Metadata = new EventMetadata
                    {
                        EventId = Guid.NewGuid(),
                        EventType = StudentEventNames.StudentRemoved,
                        Version = inboundEnvelope.Metadata.Version,
                        CausationId = inboundEnvelope.Metadata.EventId.ToString(),
                        ParentCorrelationId = inboundEnvelope.Metadata.ParentCorrelationId,
                    },
                    SecurityContext = inboundEnvelope.SecurityContext,
                    RequestContext = inboundEnvelope.RequestContext,
                    Content = new StudentRemovedEvent
                    {
                        StudentId = deletedStudent.Id,
                        FirstName = deletedStudent.FirstName,
                        LastName = deletedStudent.LastName,
                        DateOfBirth = deletedStudent.DateOfBirth,
                        Email = deletedStudent.Email,
                        Status = deletedStudent.Status
                    }
                };

            this.loggingBroker.LogInformation(
                $"[StudentService] Removed student {deletedStudent.Id}");

            this.loggingBroker.LogInformation(
                $"[StudentService] Emitting {outboundEnvelope.Metadata.EventType} event to " +
                    $"Substrate for student {deletedStudent.Id}");

            await this.eventSubstrateBroker.EmitAsync(
                outboundEnvelope,
                cancellationToken);

            return deletedStudent;
        }
    }
}
