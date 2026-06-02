// -----------------------------------------------------
// Copyright (c)  Christo du Toit - All rights reserved.
// -----------------------------------------------------

using StudentApp.Core.Models.Foundations.Students;
using StudentApp.Core.Models.Foundations.Students.Exceptions;

namespace StudentApp.Core.Services.Foundations.Students
{
    public sealed partial class StudentService
    {
        private static void ValidateStudentOnAdd(Student student)
        {
            ValidateStudentIsNotNull(student);

            Validate(
                (Rule: IsInvalid(student.Id), Parameter: nameof(Student.Id)),
                (Rule: IsInvalid(student.FirstName), Parameter: nameof(Student.FirstName)),
                (Rule: IsInvalid(student.LastName), Parameter: nameof(Student.LastName)),
                (Rule: IsInvalid(student.Email), Parameter: nameof(Student.Email)));
        }

        private static void ValidateStudentOnModify(Student student)
        {
            ValidateStudentIsNotNull(student);

            Validate(
                (Rule: IsInvalid(student.Id), Parameter: nameof(Student.Id)),
                (Rule: IsInvalid(student.FirstName), Parameter: nameof(Student.FirstName)),
                (Rule: IsInvalid(student.LastName), Parameter: nameof(Student.LastName)),
                (Rule: IsInvalid(student.Email), Parameter: nameof(Student.Email)));
        }

        private static void ValidateStudentId(Guid studentId) =>
            Validate((Rule: IsInvalid(studentId), Parameter: nameof(Student.Id)));

        private static void ValidateStorageStudent(Student maybeStudent, Guid studentId)
        {
            if (maybeStudent is null)
            {
                throw new NotFoundStudentException(
                    message: $"Could not find student with id: {studentId}.");
            }
        }

        private static void ValidateStudentIsNotNull(Student student)
        {
            if (student is null)
            {
                throw new NullStudentException(
                    message: "Student is null.");
            }
        }

        private static dynamic IsInvalid(Guid id) => new
        {
            Condition = id == Guid.Empty,
            Message = "Id is required"
        };

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static void Validate(
            params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidStudentException =
                new InvalidStudentException(
                    message: "Invalid student. Please correct the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidStudentException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidStudentException.ThrowIfContainsErrors();
        }
    }
}
