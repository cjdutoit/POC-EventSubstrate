$file = "D:\repos\cjdutoit\POC-EventSubstrate\documentation\EventSubstrate.md"
$content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

# ── SECTION 19.2 → 19.5 (StudentService) ─────────────────────────────────────

$oldSection19 = @'
### 19.2 StudentService.cs

`StudentService.cs` contains the normal Foundation Service behaviour.

This is where public CRUD and business operations live.

The service may emit events after successful operations.

```csharp
public sealed partial class StudentService : IStudentService
{
	private readonly IStorageBroker storageBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public StudentService(
		IStorageBroker storageBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.storageBroker = storageBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public async ValueTask<Student> AddStudentAsync(
		Student student,
		CancellationToken cancellationToken = default)
	{
		Student savedStudent =
			await this.storageBroker.InsertAsync(
				student,
				cancellationToken);

		EventEnvelope<StudentCreatedEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new StudentCreatedEvent
				{
					StudentId = savedStudent.Id,
					FullName = $"{savedStudent.FirstName} {savedStudent.LastName}"
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);

		return savedStudent;
	}

	public IQueryable<Student> RetrieveAllStudents() =>
		this.storageBroker.SelectAllStudents();

	public async ValueTask<Student> RetrieveStudentByIdAsync(
		Guid studentId,
		CancellationToken cancellationToken = default)
	{
		return await this.storageBroker.SelectStudentByIdAsync(
			studentId,
			cancellationToken);
	}

	public async ValueTask<Student> ModifyStudentAsync(
		Student student,
		CancellationToken cancellationToken = default)
	{
		Student modifiedStudent =
			await this.storageBroker.UpdateAsync(
				student,
				cancellationToken);

		EventEnvelope<StudentUpdatedEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new StudentUpdatedEvent
				{
					StudentId = modifiedStudent.Id
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);

		return modifiedStudent;
	}

	public async ValueTask<Student> RemoveStudentByIdAsync(
		Guid studentId,
		CancellationToken cancellationToken = default)
	{
		Student student =
			await this.RetrieveStudentByIdAsync(
				studentId,
				cancellationToken);

		Student deletedStudent =
			await this.storageBroker.DeleteAsync(
				student,
				cancellationToken);

		EventEnvelope<StudentDeletedEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new StudentDeletedEvent
				{
					StudentId = deletedStudent.Id
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);

		return deletedStudent;
	}
}
```

### 19.3 StudentService.Substrate.cs

`StudentService.Substrate.cs` contains the internal event receiver behaviour.

The receiver is explicitly implemented so normal consumers cannot call it directly.

```csharp
public sealed partial class StudentService :
	IEventReceiver<StudentEnrolledEvent>
{
	async ValueTask
		IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
			EventEnvelope<StudentEnrolledEvent> envelope,
			CancellationToken cancellationToken)
	{
		Student student =
			await this.RetrieveStudentByIdAsync(
				envelope.Content.StudentId,
				cancellationToken);

		student.Status = "Enrolled";

		await this.ModifyStudentAsync(
			student,
			cancellationToken);
	}
}
```

The Student Service is therefore both:

```text
Emitter
-------
AddStudentAsync(...) emits StudentCreatedEvent
ModifyStudentAsync(...) emits StudentUpdatedEvent
RemoveStudentByIdAsync(...) emits StudentDeletedEvent

Receiver
--------
StudentService.Substrate.cs receives StudentEnrolledEvent
```

The public Foundation Service contract remains clean and Standard-compliant.
'@

$newSection19 = @'
### 19.2 StudentService.cs

`StudentService.cs` contains the normal Foundation Service behaviour.

This is where public CRUD and business operations live.

Each public operation wraps its body in a `TryCatch` delegate so that all validation and dependency exceptions are caught and re-thrown as typed, logged service exceptions. Validation is called before any broker interaction.

The service may emit events after successful operations.

```csharp
public sealed partial class StudentService : IStudentService
{
	private readonly IStorageBroker storageBroker;
	private readonly ILoggingBroker loggingBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public StudentService(
		IStorageBroker storageBroker,
		ILoggingBroker loggingBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.storageBroker = storageBroker;
		this.loggingBroker = loggingBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public ValueTask<Student> AddStudentAsync(
		Student student,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateStudentOnAdd(student);

			Student savedStudent =
				await this.storageBroker.InsertStudentAsync(
					student,
					cancellationToken);

			EventEnvelope<StudentCreatedEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new StudentCreatedEvent
					{
						StudentId = savedStudent.Id,
						FullName = $"{savedStudent.FirstName} {savedStudent.LastName}"
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);

			return savedStudent;
		});

	public ValueTask<IQueryable<Student>> RetrieveAllStudentsAsync(
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			return await this.storageBroker.SelectAllStudentsAsync();
		});

	public ValueTask<Student> RetrieveStudentByIdAsync(
		Guid studentId,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
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
			ValidateStudentOnModify(student);

			Student maybeStudent =
				await this.storageBroker.SelectStudentByIdAsync(
					student.Id,
					cancellationToken);

			ValidateStorageStudent(maybeStudent, student.Id);

			Student modifiedStudent =
				await this.storageBroker.UpdateStudentAsync(
					student,
					cancellationToken);

			EventEnvelope<StudentUpdatedEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new StudentUpdatedEvent
					{
						StudentId = modifiedStudent.Id
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);

			return modifiedStudent;
		});

	public ValueTask<Student> RemoveStudentByIdAsync(
		Guid studentId,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateStudentId(studentId);

			Student maybeStudent =
				await this.storageBroker.SelectStudentByIdAsync(
					studentId,
					cancellationToken);

			ValidateStorageStudent(maybeStudent, studentId);

			Student deletedStudent =
				await this.storageBroker.DeleteStudentAsync(
					maybeStudent,
					cancellationToken);

			EventEnvelope<StudentDeletedEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new StudentDeletedEvent
					{
						StudentId = deletedStudent.Id
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);

			return deletedStudent;
		});
}
```

### 19.3 StudentService.Validations.cs

`StudentService.Validations.cs` contains all structural validation helpers.

These methods throw typed inner exceptions (`NullStudentException`, `InvalidStudentException`, `NotFoundStudentException`) that the `TryCatch` block in the exceptions partial catches and wraps.

```csharp
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
				message: $"Student not found with id: {studentId}.");
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
				message: "Student is invalid, fix the errors and try again.");

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
```

### 19.4 StudentService.Exceptions.cs

`StudentService.Exceptions.cs` contains the `TryCatch` delegate and all exception wrapping helpers.

All inner exceptions are caught and promoted to typed outer exceptions (`StudentValidationException`, `StudentDependencyValidationException`, `StudentDependencyException`, `StudentServiceException`) and then logged before being re-thrown.

```csharp
public sealed partial class StudentService
{
	private delegate ValueTask<Student> ReturningStudentFunction();
	private delegate ValueTask<IQueryable<Student>> ReturningStudentsFunction();

	private async ValueTask<Student> TryCatch(
		ReturningStudentFunction returningStudentFunction)
	{
		try
		{
			return await returningStudentFunction();
		}
		catch (NullStudentException nullStudentException)
		{
			throw await CreateAndLogValidationException(nullStudentException);
		}
		catch (InvalidStudentException invalidStudentException)
		{
			throw await CreateAndLogValidationException(invalidStudentException);
		}
		catch (NotFoundStudentException notFoundStudentException)
		{
			throw await CreateAndLogValidationException(notFoundStudentException);
		}
		catch (DuplicateKeyException duplicateKeyException)
		{
			var alreadyExistsStudentException = new AlreadyExistsStudentException(
				message: "Student already exists with the same Id.",
				innerException: duplicateKeyException,
				data: duplicateKeyException.Data);

			throw await CreateAndLogDependencyValidationException(alreadyExistsStudentException);
		}
		catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
		{
			var lockedStudentException = new LockedStudentException(
				message: "Locked student record, please try again later.",
				innerException: dbUpdateConcurrencyException,
				data: dbUpdateConcurrencyException.Data);

			throw await CreateAndLogDependencyValidationException(lockedStudentException);
		}
		catch (DbUpdateException dbUpdateException)
		{
			var failedStudentStorageException = new FailedStudentStorageException(
				message: "Failed student storage error occurred, contact support.",
				innerException: dbUpdateException,
				data: dbUpdateException.Data);

			throw await CreateAndLogDependencyException(failedStudentStorageException);
		}
		catch (SqlException sqlException)
		{
			var failedStudentStorageException = new FailedStudentStorageException(
				message: "Failed student storage error occurred, contact support.",
				innerException: sqlException,
				data: sqlException.Data);

			throw await CreateAndLogCriticalDependencyException(failedStudentStorageException);
		}
		catch (Exception exception)
		{
			var failedStudentServiceException = new FailedStudentServiceException(
				message: "Failed student service error occurred, contact support.",
				innerException: exception,
				data: exception.Data);

			throw await CreateAndLogServiceException(failedStudentServiceException);
		}
	}

	private async ValueTask<IQueryable<Student>> TryCatch(
		ReturningStudentsFunction returningStudentsFunction)
	{
		try
		{
			return await returningStudentsFunction();
		}
		catch (SqlException sqlException)
		{
			var failedStudentStorageException = new FailedStudentStorageException(
				message: "Failed student storage error occurred, contact support.",
				innerException: sqlException,
				data: sqlException.Data);

			throw await CreateAndLogCriticalDependencyException(failedStudentStorageException);
		}
		catch (Exception exception)
		{
			var failedStudentServiceException = new FailedStudentServiceException(
				message: "Failed student service error occurred, contact support.",
				innerException: exception,
				data: exception.Data);

			throw await CreateAndLogServiceException(failedStudentServiceException);
		}
	}

	private async ValueTask<StudentValidationException> CreateAndLogValidationException(
		Xeption exception)
	{
		var studentValidationException = new StudentValidationException(
			message: "Student validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(studentValidationException);

		return studentValidationException;
	}

	private async ValueTask<StudentDependencyValidationException>
		CreateAndLogDependencyValidationException(Xeption exception)
	{
		var studentDependencyValidationException = new StudentDependencyValidationException(
			message: "Student dependency validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(studentDependencyValidationException);

		return studentDependencyValidationException;
	}

	private async ValueTask<StudentDependencyException> CreateAndLogDependencyException(
		Xeption exception)
	{
		var studentDependencyException = new StudentDependencyException(
			message: "Student dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(studentDependencyException);

		return studentDependencyException;
	}

	private async ValueTask<StudentDependencyException> CreateAndLogCriticalDependencyException(
		Xeption exception)
	{
		var studentDependencyException = new StudentDependencyException(
			message: "Student dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogCriticalAsync(studentDependencyException);

		return studentDependencyException;
	}

	private async ValueTask<StudentServiceException> CreateAndLogServiceException(
		Xeption exception)
	{
		var studentServiceException = new StudentServiceException(
			message: "Student service error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(studentServiceException);

		return studentServiceException;
	}
}
```

### 19.5 StudentService.Substrate.cs

`StudentService.Substrate.cs` contains the internal event receiver behaviour.

The receiver is explicitly implemented so normal consumers cannot call it directly.

```csharp
public sealed partial class StudentService :
	IEventReceiver<StudentEnrolledEvent>
{
	async ValueTask
		IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
			EventEnvelope<StudentEnrolledEvent> envelope,
			CancellationToken cancellationToken)
	{
		Student student =
			await this.RetrieveStudentByIdAsync(
				envelope.Content.StudentId,
				cancellationToken);

		student.Status = "Enrolled";

		await this.ModifyStudentAsync(
			student,
			cancellationToken);
	}
}
```

The Student Service is therefore both:

```text
Emitter
-------
AddStudentAsync(...) emits StudentCreatedEvent
ModifyStudentAsync(...) emits StudentUpdatedEvent
RemoveStudentByIdAsync(...) emits StudentDeletedEvent

Receiver
--------
StudentService.Substrate.cs receives StudentEnrolledEvent
```

The public Foundation Service contract remains clean and Standard-compliant.
'@

# ── SECTION 20: EnrollmentService ────────────────────────────────────────────

$oldSection20 = @'
### 20.2 EnrollmentService.cs

```csharp
public sealed partial class EnrollmentService : IEnrollmentService
{
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public EnrollmentService(
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public async ValueTask EnrollStudentAsync(
		Guid studentId,
		string courseCode,
		CancellationToken cancellationToken = default)
	{
		// Save enrollment record here.

		EventEnvelope<StudentEnrolledEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new StudentEnrolledEvent
				{
					StudentId = studentId,
					CourseCode = courseCode
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);
	}
}
```

### 20.3 EnrollmentService.Substrate.cs

```csharp
public sealed partial class EnrollmentService :
	IEventReceiver<StudentCreatedEvent>
{
	async ValueTask
		IEventReceiver<StudentCreatedEvent>.ReceiveAsync(
			EventEnvelope<StudentCreatedEvent> envelope,
			CancellationToken cancellationToken)
	{
		// Optional automatic reaction.
		// Only do this if enrollment is reactive, not part of the required orchestration flow.

		await this.EnrollStudentAsync(
			envelope.Content.StudentId,
			"MATH-101",
			cancellationToken);
	}
}
```

Important: if enrollment is required in the registration transaction, the orchestration service should call it directly. If enrollment is optional, default, reactive, or eventually consistent, it can happen from the substrate receiver.
'@

$newSection20 = @'
### 20.2 EnrollmentService.cs

Each public operation uses `TryCatch` for consistent exception wrapping and logs errors through `ILoggingBroker`.

```csharp
public sealed partial class EnrollmentService : IEnrollmentService
{
	private readonly IStorageBroker storageBroker;
	private readonly ILoggingBroker loggingBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public EnrollmentService(
		IStorageBroker storageBroker,
		ILoggingBroker loggingBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.storageBroker = storageBroker;
		this.loggingBroker = loggingBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public ValueTask EnrollStudentAsync(
		Guid studentId,
		string courseCode,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateEnrollmentOnAdd(studentId, courseCode);

			await this.storageBroker.InsertEnrollmentAsync(
				studentId,
				courseCode,
				cancellationToken);

			EventEnvelope<StudentEnrolledEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new StudentEnrolledEvent
					{
						StudentId = studentId,
						CourseCode = courseCode
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);
		});
}
```

### 20.3 EnrollmentService.Validations.cs

```csharp
public sealed partial class EnrollmentService
{
	private static void ValidateEnrollmentOnAdd(Guid studentId, string courseCode)
	{
		Validate(
			(Rule: IsInvalid(studentId), Parameter: nameof(studentId)),
			(Rule: IsInvalid(courseCode), Parameter: nameof(courseCode)));
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
		var invalidEnrollmentException =
			new InvalidEnrollmentException(
				message: "Enrollment is invalid, fix the errors and try again.");

		foreach ((dynamic rule, string parameter) in validations)
		{
			if (rule.Condition)
			{
				invalidEnrollmentException.UpsertDataList(
					key: parameter,
					value: rule.Message);
			}
		}

		invalidEnrollmentException.ThrowIfContainsErrors();
	}
}
```

### 20.4 EnrollmentService.Exceptions.cs

```csharp
public sealed partial class EnrollmentService
{
	private delegate ValueTask ReturningNothingFunction();

	private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
	{
		try
		{
			await returningNothingFunction();
		}
		catch (InvalidEnrollmentException invalidEnrollmentException)
		{
			throw await CreateAndLogValidationException(invalidEnrollmentException);
		}
		catch (DuplicateKeyException duplicateKeyException)
		{
			var alreadyExistsEnrollmentException = new AlreadyExistsEnrollmentException(
				message: "Enrollment already exists.",
				innerException: duplicateKeyException,
				data: duplicateKeyException.Data);

			throw await CreateAndLogDependencyValidationException(alreadyExistsEnrollmentException);
		}
		catch (DbUpdateException dbUpdateException)
		{
			var failedEnrollmentStorageException = new FailedEnrollmentStorageException(
				message: "Failed enrollment storage error occurred, contact support.",
				innerException: dbUpdateException,
				data: dbUpdateException.Data);

			throw await CreateAndLogDependencyException(failedEnrollmentStorageException);
		}
		catch (SqlException sqlException)
		{
			var failedEnrollmentStorageException = new FailedEnrollmentStorageException(
				message: "Failed enrollment storage error occurred, contact support.",
				innerException: sqlException,
				data: sqlException.Data);

			throw await CreateAndLogCriticalDependencyException(failedEnrollmentStorageException);
		}
		catch (Exception exception)
		{
			var failedEnrollmentServiceException = new FailedEnrollmentServiceException(
				message: "Failed enrollment service error occurred, contact support.",
				innerException: exception,
				data: exception.Data);

			throw await CreateAndLogServiceException(failedEnrollmentServiceException);
		}
	}

	private async ValueTask<EnrollmentValidationException> CreateAndLogValidationException(
		Xeption exception)
	{
		var enrollmentValidationException = new EnrollmentValidationException(
			message: "Enrollment validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(enrollmentValidationException);

		return enrollmentValidationException;
	}

	private async ValueTask<EnrollmentDependencyValidationException>
		CreateAndLogDependencyValidationException(Xeption exception)
	{
		var enrollmentDependencyValidationException = new EnrollmentDependencyValidationException(
			message: "Enrollment dependency validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(enrollmentDependencyValidationException);

		return enrollmentDependencyValidationException;
	}

	private async ValueTask<EnrollmentDependencyException> CreateAndLogDependencyException(
		Xeption exception)
	{
		var enrollmentDependencyException = new EnrollmentDependencyException(
			message: "Enrollment dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(enrollmentDependencyException);

		return enrollmentDependencyException;
	}

	private async ValueTask<EnrollmentDependencyException> CreateAndLogCriticalDependencyException(
		Xeption exception)
	{
		var enrollmentDependencyException = new EnrollmentDependencyException(
			message: "Enrollment dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogCriticalAsync(enrollmentDependencyException);

		return enrollmentDependencyException;
	}

	private async ValueTask<EnrollmentServiceException> CreateAndLogServiceException(
		Xeption exception)
	{
		var enrollmentServiceException = new EnrollmentServiceException(
			message: "Enrollment service error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(enrollmentServiceException);

		return enrollmentServiceException;
	}
}
```

### 20.5 EnrollmentService.Substrate.cs

```csharp
public sealed partial class EnrollmentService :
	IEventReceiver<StudentCreatedEvent>
{
	async ValueTask
		IEventReceiver<StudentCreatedEvent>.ReceiveAsync(
			EventEnvelope<StudentCreatedEvent> envelope,
			CancellationToken cancellationToken)
	{
		// Optional automatic reaction.
		// Only do this if enrollment is reactive, not part of the required orchestration flow.

		await this.EnrollStudentAsync(
			envelope.Content.StudentId,
			"MATH-101",
			cancellationToken);
	}
}
```

Important: if enrollment is required in the registration transaction, the orchestration service should call it directly. If enrollment is optional, default, reactive, or eventually consistent, it can happen from the substrate receiver.
'@

# ── SECTION 21: TimetableService ─────────────────────────────────────────────

$oldSection21 = @'
### 21.2 TimetableService.cs

```csharp
public sealed partial class TimetableService : ITimetableService
{
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public TimetableService(
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public async ValueTask GenerateTimetableAsync(
		Guid studentId,
		CancellationToken cancellationToken = default)
	{
		// Generate timetable here.

		EventEnvelope<TimetableGeneratedEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new TimetableGeneratedEvent
				{
					StudentId = studentId
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);
	}
}
```

### 21.3 TimetableService.Substrate.cs

```csharp
public sealed partial class TimetableService :
	IEventReceiver<StudentEnrolledEvent>
{
	async ValueTask
		IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
			EventEnvelope<StudentEnrolledEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.GenerateTimetableAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}
}
```
'@

$newSection21 = @'
### 21.2 TimetableService.cs

Each public operation uses `TryCatch` for consistent exception wrapping and logs errors through `ILoggingBroker`.

```csharp
public sealed partial class TimetableService : ITimetableService
{
	private readonly IStorageBroker storageBroker;
	private readonly ILoggingBroker loggingBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public TimetableService(
		IStorageBroker storageBroker,
		ILoggingBroker loggingBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.storageBroker = storageBroker;
		this.loggingBroker = loggingBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public ValueTask GenerateTimetableAsync(
		Guid studentId,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateTimetableOnGenerate(studentId);

			await this.storageBroker.InsertTimetableAsync(
				studentId,
				cancellationToken);

			EventEnvelope<TimetableGeneratedEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new TimetableGeneratedEvent
					{
						StudentId = studentId
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);
		});
}
```

### 21.3 TimetableService.Validations.cs

```csharp
public sealed partial class TimetableService
{
	private static void ValidateTimetableOnGenerate(Guid studentId) =>
		Validate((Rule: IsInvalid(studentId), Parameter: nameof(studentId)));

	private static dynamic IsInvalid(Guid id) => new
	{
		Condition = id == Guid.Empty,
		Message = "Id is required"
	};

	private static void Validate(
		params (dynamic Rule, string Parameter)[] validations)
	{
		var invalidTimetableException =
			new InvalidTimetableException(
				message: "Timetable is invalid, fix the errors and try again.");

		foreach ((dynamic rule, string parameter) in validations)
		{
			if (rule.Condition)
			{
				invalidTimetableException.UpsertDataList(
					key: parameter,
					value: rule.Message);
			}
		}

		invalidTimetableException.ThrowIfContainsErrors();
	}
}
```

### 21.4 TimetableService.Exceptions.cs

```csharp
public sealed partial class TimetableService
{
	private delegate ValueTask ReturningNothingFunction();

	private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
	{
		try
		{
			await returningNothingFunction();
		}
		catch (InvalidTimetableException invalidTimetableException)
		{
			throw await CreateAndLogValidationException(invalidTimetableException);
		}
		catch (DbUpdateException dbUpdateException)
		{
			var failedTimetableStorageException = new FailedTimetableStorageException(
				message: "Failed timetable storage error occurred, contact support.",
				innerException: dbUpdateException,
				data: dbUpdateException.Data);

			throw await CreateAndLogDependencyException(failedTimetableStorageException);
		}
		catch (SqlException sqlException)
		{
			var failedTimetableStorageException = new FailedTimetableStorageException(
				message: "Failed timetable storage error occurred, contact support.",
				innerException: sqlException,
				data: sqlException.Data);

			throw await CreateAndLogCriticalDependencyException(failedTimetableStorageException);
		}
		catch (Exception exception)
		{
			var failedTimetableServiceException = new FailedTimetableServiceException(
				message: "Failed timetable service error occurred, contact support.",
				innerException: exception,
				data: exception.Data);

			throw await CreateAndLogServiceException(failedTimetableServiceException);
		}
	}

	private async ValueTask<TimetableValidationException> CreateAndLogValidationException(
		Xeption exception)
	{
		var timetableValidationException = new TimetableValidationException(
			message: "Timetable validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(timetableValidationException);

		return timetableValidationException;
	}

	private async ValueTask<TimetableDependencyException> CreateAndLogDependencyException(
		Xeption exception)
	{
		var timetableDependencyException = new TimetableDependencyException(
			message: "Timetable dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(timetableDependencyException);

		return timetableDependencyException;
	}

	private async ValueTask<TimetableDependencyException> CreateAndLogCriticalDependencyException(
		Xeption exception)
	{
		var timetableDependencyException = new TimetableDependencyException(
			message: "Timetable dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogCriticalAsync(timetableDependencyException);

		return timetableDependencyException;
	}

	private async ValueTask<TimetableServiceException> CreateAndLogServiceException(
		Xeption exception)
	{
		var timetableServiceException = new TimetableServiceException(
			message: "Timetable service error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(timetableServiceException);

		return timetableServiceException;
	}
}
```

### 21.5 TimetableService.Substrate.cs

```csharp
public sealed partial class TimetableService :
	IEventReceiver<StudentEnrolledEvent>
{
	async ValueTask
		IEventReceiver<StudentEnrolledEvent>.ReceiveAsync(
			EventEnvelope<StudentEnrolledEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.GenerateTimetableAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}
}
```
'@

# ── SECTION 22: NotificationService ──────────────────────────────────────────

$oldSection22 = @'
### 22.2 NotificationService.cs

```csharp
public sealed partial class NotificationService : INotificationService
{
	private readonly IEmailBroker emailBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public NotificationService(
		IEmailBroker emailBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.emailBroker = emailBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public async ValueTask SendWelcomeEmailAsync(
		Guid studentId,
		CancellationToken cancellationToken = default)
	{
		await this.emailBroker.SendWelcomeEmailAsync(
			studentId,
			cancellationToken);

		EventEnvelope<WelcomeEmailSentEvent> envelope =
			await this.envelopeFactory.CreateAsync(
				new WelcomeEmailSentEvent
				{
					StudentId = studentId,
					EmailAddress = "student@example.com"
				},
				cancellationToken);

		await this.eventSubstrate.PublishAsync(
			envelope,
			cancellationToken);
	}

	public async ValueTask SendTimetableEmailAsync(
		Guid studentId,
		CancellationToken cancellationToken = default)
	{
		await this.emailBroker.SendTimetableEmailAsync(
			studentId,
			cancellationToken);
	}
}
```

### 22.3 NotificationService.Substrate.cs

```csharp
public sealed partial class NotificationService :
	IEventReceiver<StudentCreatedEvent>,
	IEventReceiver<TimetableGeneratedEvent>
{
	async ValueTask
		IEventReceiver<StudentCreatedEvent>.ReceiveAsync(
			EventEnvelope<StudentCreatedEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.SendWelcomeEmailAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}

	async ValueTask
		IEventReceiver<TimetableGeneratedEvent>.ReceiveAsync(
			EventEnvelope<TimetableGeneratedEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.SendTimetableEmailAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}
}
```
'@

$newSection22 = @'
### 22.2 NotificationService.cs

Each public operation uses `TryCatch` for consistent exception wrapping and logs errors through `ILoggingBroker`.

```csharp
public sealed partial class NotificationService : INotificationService
{
	private readonly IEmailBroker emailBroker;
	private readonly ILoggingBroker loggingBroker;
	private readonly IEventSubstrate eventSubstrate;
	private readonly IEventEnvelopeFactory envelopeFactory;

	public NotificationService(
		IEmailBroker emailBroker,
		ILoggingBroker loggingBroker,
		IEventSubstrate eventSubstrate,
		IEventEnvelopeFactory envelopeFactory)
	{
		this.emailBroker = emailBroker;
		this.loggingBroker = loggingBroker;
		this.eventSubstrate = eventSubstrate;
		this.envelopeFactory = envelopeFactory;
	}

	public ValueTask SendWelcomeEmailAsync(
		Guid studentId,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateStudentId(studentId);

			await this.emailBroker.SendWelcomeEmailAsync(
				studentId,
				cancellationToken);

			EventEnvelope<WelcomeEmailSentEvent> envelope =
				await this.envelopeFactory.CreateAsync(
					new WelcomeEmailSentEvent
					{
						StudentId = studentId,
						EmailAddress = "student@example.com"
					},
					cancellationToken);

			await this.eventSubstrate.PublishAsync(
				envelope,
				cancellationToken);
		});

	public ValueTask SendTimetableEmailAsync(
		Guid studentId,
		CancellationToken cancellationToken = default) =>
		TryCatch(async () =>
		{
			ValidateStudentId(studentId);

			await this.emailBroker.SendTimetableEmailAsync(
				studentId,
				cancellationToken);
		});
}
```

### 22.3 NotificationService.Validations.cs

```csharp
public sealed partial class NotificationService
{
	private static void ValidateStudentId(Guid studentId) =>
		Validate((Rule: IsInvalid(studentId), Parameter: nameof(studentId)));

	private static dynamic IsInvalid(Guid id) => new
	{
		Condition = id == Guid.Empty,
		Message = "Id is required"
	};

	private static void Validate(
		params (dynamic Rule, string Parameter)[] validations)
	{
		var invalidNotificationException =
			new InvalidNotificationException(
				message: "Notification is invalid, fix the errors and try again.");

		foreach ((dynamic rule, string parameter) in validations)
		{
			if (rule.Condition)
			{
				invalidNotificationException.UpsertDataList(
					key: parameter,
					value: rule.Message);
			}
		}

		invalidNotificationException.ThrowIfContainsErrors();
	}
}
```

### 22.4 NotificationService.Exceptions.cs

```csharp
public sealed partial class NotificationService
{
	private delegate ValueTask ReturningNothingFunction();

	private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
	{
		try
		{
			await returningNothingFunction();
		}
		catch (InvalidNotificationException invalidNotificationException)
		{
			throw await CreateAndLogValidationException(invalidNotificationException);
		}
		catch (HttpRequestException httpRequestException)
		{
			var failedNotificationDependencyException = new FailedNotificationDependencyException(
				message: "Failed notification dependency error occurred, contact support.",
				innerException: httpRequestException,
				data: httpRequestException.Data);

			throw await CreateAndLogDependencyException(failedNotificationDependencyException);
		}
		catch (Exception exception)
		{
			var failedNotificationServiceException = new FailedNotificationServiceException(
				message: "Failed notification service error occurred, contact support.",
				innerException: exception,
				data: exception.Data);

			throw await CreateAndLogServiceException(failedNotificationServiceException);
		}
	}

	private async ValueTask<NotificationValidationException> CreateAndLogValidationException(
		Xeption exception)
	{
		var notificationValidationException = new NotificationValidationException(
			message: "Notification validation error occurred, fix the errors and try again.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(notificationValidationException);

		return notificationValidationException;
	}

	private async ValueTask<NotificationDependencyException> CreateAndLogDependencyException(
		Xeption exception)
	{
		var notificationDependencyException = new NotificationDependencyException(
			message: "Notification dependency error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(notificationDependencyException);

		return notificationDependencyException;
	}

	private async ValueTask<NotificationServiceException> CreateAndLogServiceException(
		Xeption exception)
	{
		var notificationServiceException = new NotificationServiceException(
			message: "Notification service error occurred, contact support.",
			innerException: exception);

		await this.loggingBroker.LogErrorAsync(notificationServiceException);

		return notificationServiceException;
	}
}
```

### 22.5 NotificationService.Substrate.cs

```csharp
public sealed partial class NotificationService :
	IEventReceiver<StudentCreatedEvent>,
	IEventReceiver<TimetableGeneratedEvent>
{
	async ValueTask
		IEventReceiver<StudentCreatedEvent>.ReceiveAsync(
			EventEnvelope<StudentCreatedEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.SendWelcomeEmailAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}

	async ValueTask
		IEventReceiver<TimetableGeneratedEvent>.ReceiveAsync(
			EventEnvelope<TimetableGeneratedEvent> envelope,
			CancellationToken cancellationToken)
	{
		await this.SendTimetableEmailAsync(
			envelope.Content.StudentId,
			cancellationToken);
	}
}
```
'@

# Apply replacements
$content = $content.Replace($oldSection19, $newSection19)
$content = $content.Replace($oldSection20, $newSection20)
$content = $content.Replace($oldSection21, $newSection21)
$content = $content.Replace($oldSection22, $newSection22)

[System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
Write-Host "Done. New length: $($content.Length)"
