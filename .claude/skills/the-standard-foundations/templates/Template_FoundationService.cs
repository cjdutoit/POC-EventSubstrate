// ---
// skill: the-standard-foundations
// type: template
// source-section: "2.1 Foundation Services"
// source-truth: ContentItemService (all partials) + all ContentItemServiceTests (all partials)
// last-updated: 2025
// ---

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 1: INTERFACE
// ═══════════════════════════════════════════════════════════════════════════════

// I{Entity}Service.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {Namespace}.Models.Foundations.{Entity}s;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public interface I{Entity}Service
    {
        ValueTask<{Entity}> Add{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<{Entity}>> RetrieveAll{Entity}sAsync(
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Retrieve{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Modify{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Remove{Entity}ByIdAsync(
            Guid {entity}Id,
            string? deletionReason = null,
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> HardRemove{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default);
    }
}

// NOTE: The interface is a clean contract — no implementation details.
// Every method accepts an optional CancellationToken (default) and returns a ValueTask.

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 2: SERVICE IMPLEMENTATION
// ═══════════════════════════════════════════════════════════════════════════════

// {Entity}Service.cs — main partial
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {Namespace}.Brokers.DateTimes;
using {Namespace}.Brokers.Events;
using {Namespace}.Brokers.Loggings;
using {Namespace}.Brokers.Securities;
using {Namespace}.Brokers.Storages.Sql;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service : I{Entity}Service
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly IEventBroker eventBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public {Entity}Service(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            IEventBroker eventBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.eventBroker = eventBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<{Entity}> Add{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                {entity} = await this.securityAuditBroker.ApplyAddAuditValuesAsync({entity});
                await ValidateOnAdd{Entity}({entity});

                {Entity} added{Entity} =
                    await this.storageBroker.Insert{Entity}Async({entity}, cancellationToken);

                var envelope = new EventEnvelope<{Entity}> { Content = added{Entity} };
                await this.eventBroker.Publish{Entity}Async(envelope, "{Entity}Added");

                return added{Entity};
            });

        public ValueTask<IQueryable<{Entity}>> RetrieveAll{Entity}sAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await this.storageBroker.SelectAll{Entity}sAsync();
            });

        public ValueTask<{Entity}> Retrieve{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateOnRetrieve{Entity}ById({entity}Id);

                {Entity} maybe{Entity} =
                    await this.storageBroker.Select{Entity}ByIdAsync(
                        {entity}Id,
                        cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                return maybe{Entity};
            });

        public ValueTask<{Entity}> Modify{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                {entity} = await this.securityAuditBroker.ApplyModifyAuditValuesAsync({entity});
                await ValidateOnModify{Entity}({entity});

                {Entity} maybe{Entity} =
                    await this.storageBroker.Select{Entity}ByIdAsync({entity}.Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}.Id);

                {entity} = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync({entity}, maybe{Entity});

                ValidateAgainstStorage{Entity}OnModify(
                    input{Entity}: {entity},
                    storage{Entity}: maybe{Entity});

                {Entity} updated{Entity} =
                    await this.storageBroker.Update{Entity}Async({entity}, cancellationToken);

                var envelope = new EventEnvelope<{Entity}> { Content = updated{Entity} };
                await this.eventBroker.Publish{Entity}Async(envelope, "{Entity}Modified");

                return updated{Entity};
            });

        public ValueTask<{Entity}> Remove{Entity}ByIdAsync(
            Guid {entity}Id,
            string? deletionReason = null,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateOnRemove{Entity}ById({entity}Id);

                {Entity} maybe{Entity} =
                    await this.storageBroker.Select{Entity}ByIdAsync({entity}Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                if (maybe{Entity}.IsDeleted)
                    return maybe{Entity};

                maybe{Entity} = await this.securityAuditBroker.ApplyRemoveAuditValuesAsync(maybe{Entity});
                maybe{Entity}.IsDeleted = true;
                maybe{Entity}.DeletionReason = deletionReason;

                {Entity} deleted{Entity} =
                    await this.storageBroker.Update{Entity}Async(maybe{Entity}, cancellationToken);

                var envelope = new EventEnvelope<{Entity}> { Content = deleted{Entity} };
                await this.eventBroker.Publish{Entity}Async(envelope, "{Entity}Removed");

                return deleted{Entity};
            });

        public ValueTask<{Entity}> HardRemove{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateOnRemove{Entity}ById({entity}Id);

                {Entity} maybe{Entity} =
                    await this.storageBroker.Select{Entity}ByIdAsync({entity}Id, cancellationToken);

                ValidateStorage{Entity}(maybe{Entity}, {entity}Id);

                {Entity} deleted{Entity} =
                    await this.storageBroker.Delete{Entity}Async(maybe{Entity}, cancellationToken);

                var envelope = new EventEnvelope<{Entity}> { Content = deleted{Entity} };
                await this.eventBroker.Publish{Entity}Async(envelope, "{Entity}Removed");

                return deleted{Entity};
            });
    }
}

// {Entity}Service.Validations.cs — validation partial
using System;
using System.Threading.Tasks;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service
    {
        private async ValueTask ValidateOnAdd{Entity}({Entity} {entity})
        {
            Validate{Entity}IsNotNull({entity});
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                message: "{Entity human-readable name} is invalid, fix the errors and try again.",
                (Rule: IsInvalid({entity}.Id), Parameter: nameof({Entity}.Id)),
                // NOTE: Add all required field validations for the entity here.
                // Common audit fields always validated:
                (Rule: IsInvalid({entity}.CreatedBy), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsInvalid({entity}.UpdatedBy), Parameter: nameof({Entity}.UpdatedBy)),
                (Rule: IsInvalid({entity}.CreatedWhen), Parameter: nameof({Entity}.CreatedWhen)),
                (Rule: IsInvalid({entity}.UpdatedWhen), Parameter: nameof({Entity}.UpdatedWhen)),

                (Rule: IsGreaterThan({entity}.CreatedBy, 255),
                    Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsGreaterThan({entity}.UpdatedBy, 255),
                    Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsNotSame(
                        firstDate: {entity}.UpdatedWhen,
                        secondDate: {entity}.CreatedWhen,
                        secondDateName: nameof({Entity}.CreatedWhen)),
                    Parameter: nameof({Entity}.UpdatedWhen)),

                (Rule: IsNotSame(
                        first: currentUserId,
                        second: {entity}.CreatedBy),
                    Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsNotSame(
                        first: {entity}.UpdatedBy,
                        second: {entity}.CreatedBy,
                        secondName: nameof({Entity}.CreatedBy)),
                    Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: await IsNotRecentAsync({entity}.CreatedWhen),
                    Parameter: nameof({Entity}.CreatedWhen)));
        }

        private async ValueTask ValidateOnModify{Entity}({Entity} {entity})
        {
            Validate{Entity}IsNotNull({entity});
            string currentUserId = await this.securityAuditBroker.GetUserIdAsync();

            Validate(
                message: "{Entity human-readable name} is invalid, fix the errors and try again.",
                (Rule: IsInvalid({entity}.Id), Parameter: nameof({Entity}.Id)),
                // NOTE: Add all required field validations for the entity here.
                // Common audit fields always validated:
                (Rule: IsInvalid({entity}.CreatedBy), Parameter: nameof({Entity}.CreatedBy)),
                (Rule: IsInvalid({entity}.UpdatedBy), Parameter: nameof({Entity}.UpdatedBy)),
                (Rule: IsInvalid({entity}.CreatedWhen), Parameter: nameof({Entity}.CreatedWhen)),
                (Rule: IsInvalid({entity}.UpdatedWhen), Parameter: nameof({Entity}.UpdatedWhen)),

                (Rule: IsGreaterThan({entity}.CreatedBy, 255),
                    Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsGreaterThan({entity}.UpdatedBy, 255),
                    Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsNotSame(
                        first: currentUserId,
                        second: {entity}.UpdatedBy),
                    Parameter: nameof({Entity}.UpdatedBy)),

                (Rule: IsSame(
                        firstDate: {entity}.UpdatedWhen,
                        secondDate: {entity}.CreatedWhen,
                        secondDateName: nameof({Entity}.CreatedWhen)),
                    Parameter: nameof({Entity}.UpdatedWhen)),

                (Rule: await IsNotRecentAsync({entity}.UpdatedWhen),
                    Parameter: nameof({Entity}.UpdatedWhen)));
        }

        private static void ValidateOnRetrieve{Entity}ById(Guid {entity}Id) =>
            Validate(
                message: "{Entity human-readable name} is invalid, fix the errors and try again.",
                (Rule: IsInvalid({entity}Id), Parameter: nameof({Entity}.Id)));

        private static void ValidateOnRemove{Entity}ById(Guid {entity}Id) =>
            Validate(
                message: "{Entity human-readable name} is invalid, fix the errors and try again.",
                (Rule: IsInvalid({entity}Id), Parameter: nameof({Entity}.Id)));

        private static void ValidateStorage{Entity}({Entity} maybe{Entity}, Guid {entity}Id)
        {
            if (maybe{Entity} is null)
            {
                throw new NotFound{Entity}Exception(
                    message: $"{{Entity human-readable name}} not found with id: {{entity}Id}.");
            }
        }

        private static void ValidateAgainstStorage{Entity}OnModify(
            {Entity} input{Entity},
            {Entity} storage{Entity})
        {
            Validate(
                message: "{Entity human-readable name} is invalid, fix the errors and try again.",

                (Rule: IsNotSame(
                        firstDate: input{Entity}.CreatedWhen,
                        secondDate: storage{Entity}.CreatedWhen,
                        secondDateName: nameof({Entity}.CreatedWhen)),
                    Parameter: nameof({Entity}.CreatedWhen)),

                (Rule: IsNotSame(
                        first: input{Entity}.CreatedBy,
                        second: storage{Entity}.CreatedBy,
                        secondName: nameof({Entity}.CreatedBy)),
                    Parameter: nameof({Entity}.CreatedBy)),

                (Rule: IsSame(
                        firstDate: input{Entity}.UpdatedWhen,
                        secondDate: storage{Entity}.UpdatedWhen,
                        secondDateName: nameof({Entity}.UpdatedWhen)),
                    Parameter: nameof({Entity}.UpdatedWhen)));
        }

        private static void Validate{Entity}IsNotNull({Entity} {entity})
        {
            if ({entity} is null)
            {
                throw new Null{Entity}Exception(message: "{Entity human-readable name} is null.");
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

        private static dynamic IsInvalid(DateTimeOffset date) => new
        {
            Condition = date == default,
            Message = "Date is required"
        };

        private static dynamic IsGreaterThan(string text, int maxLength) => new
        {
            Condition = IsExceedingLength(text, maxLength),
            Message = $"Text exceed max length of {maxLength} characters"
        };

        private static bool IsExceedingLength(string text, int maxLength) =>
            (text ?? string.Empty).Length > maxLength;

        private static dynamic IsNotSame(
            string first,
            string second) => new
            {
                Condition = first != second,
                Message = $"Expected value to be '{first}' but found '{second}'."
            };

        private static dynamic IsNotSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate != secondDate,
                Message = $"Date is not the same as {secondDateName}"
            };

        private static dynamic IsNotSame(
            string first,
            string second,
            string secondName) => new
            {
                Condition = first != second,
                Message = $"Text is not the same as {secondName}"
            };

        private static dynamic IsSame(
            DateTimeOffset firstDate,
            DateTimeOffset secondDate,
            string secondDateName) => new
            {
                Condition = firstDate == secondDate,
                Message = $"Date is the same as {secondDateName}"
            };

        private async ValueTask<dynamic> IsNotRecentAsync(DateTimeOffset date)
        {
            var (isNotRecent, startDate, endDate) = await IsDateNotRecentAsync(date);

            return new
            {
                Condition = isNotRecent,
                Message = $"Date is not recent. Expected a value between {startDate} and {endDate} but found {date}"
            };
        }

        private async ValueTask<(bool IsNotRecent, DateTimeOffset StartDate, DateTimeOffset EndDate)>
            IsDateNotRecentAsync(DateTimeOffset date)
        {
            int pastThreshold = 90;
            int futureThreshold = 0;
            DateTimeOffset currentDateTime = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            if (currentDateTime == default)
            {
                return (false, default, default);
            }

            DateTimeOffset startDate = currentDateTime.AddSeconds(-pastThreshold);
            DateTimeOffset endDate = currentDateTime.AddSeconds(futureThreshold);
            bool isNotRecent = date < startDate || date > endDate;

            return (isNotRecent, startDate, endDate);
        }

        private static void Validate(
            string message,
            params (dynamic Rule, string Parameter)[] validations)
        {
            var invalid{Entity}Exception = new Invalid{Entity}Exception(message);

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalid{Entity}Exception.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalid{Entity}Exception.ThrowIfContainsErrors();
        }
    }
}

// {Entity}Service.Exceptions.cs — exception handling partial
using System;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xeptions;

namespace {Namespace}.Services.Foundations.{Entity}s
{
    public partial class {Entity}Service
    {
        private delegate ValueTask<{Entity}> Returning{Entity}Function();
        private delegate ValueTask<IQueryable<{Entity}>> Returning{Entity}sFunction();

        private async ValueTask<{Entity}> TryCatch(Returning{Entity}Function returning{Entity}Function)
        {
            try
            {
                return await returning{Entity}Function();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var timeout{Entity}Exception = new Timeout{Entity}Exception(
                    message: "{Entity human-readable name} timed out, contact support.",
                    innerException: new TimeoutException(),
                    data: operationCanceledException.Data);

                throw await CreateAndLogDependencyException(timeout{Entity}Exception);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Null{Entity}Exception null{Entity}Exception)
            {
                throw await CreateAndLogValidationException(null{Entity}Exception);
            }
            catch (Invalid{Entity}Exception invalid{Entity}Exception)
            {
                throw await CreateAndLogValidationException(invalid{Entity}Exception);
            }
            catch (SqlException sqlException)
            {
                var failedStorage{Entity}Exception = new FailedStorage{Entity}Exception(
                    message: "Failed {entity} storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);

                throw await CreateAndLogCriticalDependencyException(failedStorage{Entity}Exception);
            }
            catch (NotFound{Entity}Exception notFound{Entity}Exception)
            {
                throw await CreateAndLogValidationException(notFound{Entity}Exception);
            }
            catch (DuplicateKeyException duplicateKeyException)
            {
                var alreadyExists{Entity}Exception = new AlreadyExists{Entity}Exception(
                    message: "{Entity human-readable name} already exists with the same Id.",
                    innerException: duplicateKeyException,
                    data: duplicateKeyException.Data);

                throw await CreateAndLogDependencyValidationException(alreadyExists{Entity}Exception);
            }
            catch (ForeignKeyConstraintConflictException foreignKeyConstraintConflictException)
            {
                var invalid{Entity}ReferenceException = new Invalid{Entity}ReferenceException(
                    message: "Invalid {entity} reference error occurred.",
                    innerException: foreignKeyConstraintConflictException,
                    data: foreignKeyConstraintConflictException.Data);

                throw await CreateAndLogDependencyValidationException(invalid{Entity}ReferenceException);
            }
            catch (DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                var locked{Entity}Exception = new Locked{Entity}Exception(
                    message: "Locked {entity} record, please try again later.",
                    innerException: dbUpdateConcurrencyException,
                    data: dbUpdateConcurrencyException.Data);

                throw await CreateAndLogDependencyValidationException(locked{Entity}Exception);
            }
            catch (DbUpdateException dbUpdateException)
            {
                var failedStorage{Entity}Exception = new FailedStorage{Entity}Exception(
                    message: "Failed {entity} storage error occurred, contact support.",
                    innerException: dbUpdateException,
                    data: dbUpdateException.Data);

                throw await CreateAndLogDependencyException(failedStorage{Entity}Exception);
            }
            catch (Exception exception)
            {
                var failed{Entity}ServiceException = new Failed{Entity}ServiceException(
                    message: "Failed {entity} service error occurred, please contact support.",
                    innerException: exception,
                    data: exception.Data);

                throw await CreateAndLogServiceException(failed{Entity}ServiceException);
            }
        }

        private async ValueTask<IQueryable<{Entity}>> TryCatch(
            Returning{Entity}sFunction returning{Entity}sFunction)
        {
            try
            {
                return await returning{Entity}sFunction();
            }
            catch (OperationCanceledException operationCanceledException)
                when (operationCanceledException.CancellationToken.IsCancellationRequested is false)
            {
                var timeout{Entity}Exception = new Timeout{Entity}Exception(
                    message: "{Entity human-readable name} timed out, contact support.",
                    innerException: new TimeoutException(),
                    data: operationCanceledException.Data);

                throw await CreateAndLogDependencyException(timeout{Entity}Exception);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (SqlException sqlException)
            {
                var failedStorage{Entity}Exception = new FailedStorage{Entity}Exception(
                    message: "Failed {entity} storage error occurred, contact support.",
                    innerException: sqlException,
                    data: sqlException.Data);

                throw await CreateAndLogCriticalDependencyException(failedStorage{Entity}Exception);
            }
            catch (Exception exception)
            {
                var failed{Entity}ServiceException = new Failed{Entity}ServiceException(
                    message: "Failed {entity} service error occurred, please contact support.",
                    innerException: exception,
                    data: exception.Data);

                throw await CreateAndLogServiceException(failed{Entity}ServiceException);
            }
        }

        private async ValueTask<{Entity}ValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var {entity}ValidationException = new {Entity}ValidationException(
                message: "{Entity human-readable name} validation error occurred, fix the errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ValidationException);

            return {entity}ValidationException;
        }

        private async ValueTask<{Entity}DependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var {entity}DependencyException = new {Entity}DependencyException(
                message: "{Entity human-readable name} dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogCriticalAsync({entity}DependencyException);

            return {entity}DependencyException;
        }

        private async ValueTask<{Entity}DependencyValidationException> CreateAndLogDependencyValidationException(
            Xeption exception)
        {
            var {entity}DependencyValidationException = new {Entity}DependencyValidationException(
                message: "{Entity human-readable name} dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}DependencyValidationException);

            return {entity}DependencyValidationException;
        }

        private async ValueTask<{Entity}DependencyException> CreateAndLogDependencyException(Xeption exception)
        {
            var {entity}DependencyException = new {Entity}DependencyException(
                message: "{Entity human-readable name} dependency error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}DependencyException);

            return {entity}DependencyException;
        }

        private async ValueTask<{Entity}ServiceException> CreateAndLogServiceException(Xeption exception)
        {
            var {entity}ServiceException = new {Entity}ServiceException(
                message: "{Entity human-readable name} service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync({entity}ServiceException);

            return {entity}ServiceException;
        }
    }
}

// NOTE: Key structural rules for the Exceptions partial:
// 1. TimeoutException is instantiated inline as `new TimeoutException()` (no separate wrapper variable).
// 2. Exception variables are declared inline: `var fooException = new FooException(...)` on one statement.
// 3. Catch order in the single-entity TryCatch:
//    OperationCanceledException (timeout) → OperationCanceledException (rethrow) →
//    NullXxxException → InvalidXxxException → SqlException →
//    NotFoundXxxException → DuplicateKeyException → ForeignKeyConstraintConflictException →
//    DbUpdateConcurrencyException → DbUpdateException → Exception
// 4. The IQueryable TryCatch omits all validation catches (Null, Invalid, NotFound).
// 5. CreateAndLogCriticalDependencyException uses LogCriticalAsync; all others use LogErrorAsync.

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 3: EXCEPTION MODELS
// ═══════════════════════════════════════════════════════════════════════════════

// Null{Entity}Exception.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Null{Entity}Exception : Xeption
    {
        public Null{Entity}Exception(string message)
            : base(message)
        { }
    }
}

// Invalid{Entity}Exception.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Invalid{Entity}Exception : Xeption
    {
        public Invalid{Entity}Exception(string message)
            : base(message)
        { }
    }
}

// NotFound{Entity}Exception.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class NotFound{Entity}Exception : Xeption
    {
        public NotFound{Entity}Exception(string message)
            : base(message)
        { }
    }
}

// AlreadyExists{Entity}Exception.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class AlreadyExists{Entity}Exception : Xeption
    {
        public AlreadyExists{Entity}Exception(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Invalid{Entity}ReferenceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Invalid{Entity}ReferenceException : Xeption
    {
        public Invalid{Entity}ReferenceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Locked{Entity}Exception.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Locked{Entity}Exception : Xeption
    {
        public Locked{Entity}Exception(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// FailedStorage{Entity}Exception.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class FailedStorage{Entity}Exception : Xeption
    {
        public FailedStorage{Entity}Exception(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Failed{Entity}ServiceException.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Failed{Entity}ServiceException : Xeption
    {
        public Failed{Entity}ServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// Timeout{Entity}Exception.cs
using System;
using System.Collections;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class Timeout{Entity}Exception : Xeption
    {
        public Timeout{Entity}Exception(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}

// {Entity}ValidationException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ValidationException : Xeption
    {
        public {Entity}ValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}DependencyValidationException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}DependencyValidationException : Xeption
    {
        public {Entity}DependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}DependencyException.cs
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}DependencyException : Xeption
    {
        public {Entity}DependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}

// {Entity}ServiceException.cs
using System;
using Xeptions;

namespace {Namespace}.Models.Foundations.{Entity}s.Exceptions
{
    public class {Entity}ServiceException : Xeption
    {
        public {Entity}ServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}

// NOTE: Exception naming conventions:
// - "Leaf" exceptions (cause-level): Null, Invalid, NotFound, AlreadyExists,
//   InvalidReference, Locked, FailedStorage, FailedService, Timeout.
//   These accept `Exception innerException` + `IDictionary data` where relevant.
// - "Wrapper" exceptions (category-level): ValidationException, DependencyValidationException,
//   DependencyException accept `Xeption innerException`.
// - ServiceException accepts `Exception innerException` (not Xeption) to allow wrapping
//   any exception type at the service boundary.

// ═══════════════════════════════════════════════════════════════════════════════
// SECTION 4: UNIT TESTS
// ═══════════════════════════════════════════════════════════════════════════════

// {Entity}ServiceTests.cs — test fixture base
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using EFxceptions.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using {Namespace}.Brokers.DateTimes;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using {Namespace}.Brokers.Events;
using {Namespace}.Brokers.Loggings;
using {Namespace}.Brokers.Securities;
using {Namespace}.Brokers.Storages.Sql;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Services.Foundations.{Entity}s;
using Microsoft.Data.SqlClient;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<IEventBroker> eventBrokerMock;
        private readonly Mock<ISecurityAuditBroker> securityAuditBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly I{Entity}Service {entity}Service;

        public {Entity}ServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.eventBrokerMock = new Mock<IEventBroker>();
            this.securityAuditBrokerMock = new Mock<ISecurityAuditBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.{entity}Service = new {Entity}Service(
                storageBroker: this.storageBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                eventBroker: this.eventBrokerMock.Object,
                securityAuditBroker: this.securityAuditBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static IQueryable<{Entity}> CreateRandom{Entity}s()
        {
            return Create{Entity}Filler(dateTimeOffset: GetRandomDateTimeOffset())
                .Create(count: GetRandomNumber())
                .AsQueryable();
        }

        private static {Entity} CreateRandomModify{Entity}(
            DateTimeOffset dateTimeOffset,
            string userId = "")
        {
            int randomDaysInPast = GetRandomNegativeNumber();
            {Entity} random{Entity} = Create{Entity}Filler(dateTimeOffset, userId).Create();
            random{Entity}.CreatedWhen = random{Entity}.CreatedWhen.AddDays(randomDaysInPast);

            return random{Entity};
        }

        public static TheoryData<int> MinutesBeforeOrAfter()
        {
            int randomTimeInFuture = GetRandomNumber();
            int randomTimeInPast = GetRandomNegativeNumber();

            return new TheoryData<int>
            {
                randomTimeInFuture,
                randomTimeInPast
            };
        }

        public static TheoryData<Exception, Xeption> DependencyValidationExceptions()
        {
            string someMessage = GetRandomString();
            var duplicateKeyException = new DuplicateKeyException(someMessage);
            var foreignKeyConstraintConflictException = new ForeignKeyConstraintConflictException(someMessage);

            return new TheoryData<Exception, Xeption>
            {
                {
                    duplicateKeyException,
                    new AlreadyExists{Entity}Exception(
                        message: "{Entity human-readable name} already exists with the same Id.",
                        innerException: duplicateKeyException,
                        data: duplicateKeyException.Data)
                },
                {
                    foreignKeyConstraintConflictException,
                    new Invalid{Entity}ReferenceException(
                        message: "Invalid {entity} reference error occurred.",
                        innerException: foreignKeyConstraintConflictException,
                        data: foreignKeyConstraintConflictException.Data)
                }
            };
        }

        public static TheoryData<Exception, Xeption> ModifyDependencyValidationExceptions()
        {
            string someMessage = GetRandomString();
            var dbUpdateConcurrencyException = new DbUpdateConcurrencyException();
            var foreignKeyConstraintConflictException = new ForeignKeyConstraintConflictException(someMessage);

            return new TheoryData<Exception, Xeption>
            {
                {
                    dbUpdateConcurrencyException,
                    new Locked{Entity}Exception(
                        message: "Locked {entity} record, please try again later.",
                        innerException: dbUpdateConcurrencyException,
                        data: dbUpdateConcurrencyException.Data)
                },
                {
                    foreignKeyConstraintConflictException,
                    new Invalid{Entity}ReferenceException(
                        message: "Invalid {entity} reference error occurred.",
                        innerException: foreignKeyConstraintConflictException,
                        data: foreignKeyConstraintConflictException.Data)
                }
            };
        }

        public static TheoryData<Exception, Xeption> DependencyExceptions()
        {
            var operationCanceledException = new OperationCanceledException();
            var dbUpdateException = new DbUpdateException();

            return new TheoryData<Exception, Xeption>
            {
                {
                    operationCanceledException,
                    new Timeout{Entity}Exception(
                        message: "{Entity human-readable name} timed out, contact support.",
                        innerException: new TimeoutException(),
                        data: operationCanceledException.Data)
                },
                {
                    dbUpdateException,
                    new FailedStorage{Entity}Exception(
                        message: "Failed {entity} storage error occurred, contact support.",
                        innerException: dbUpdateException,
                        data: dbUpdateException.Data)
                }
            };
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static SqlException GetSqlException() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static {Entity} CreateRandom{Entity}() =>
            Create{Entity}Filler(dateTimeOffset: GetRandomDateTimeOffset()).Create();

        private static {Entity} CreateRandom{Entity}(DateTimeOffset dateTimeOffset, string userId = "") =>
            Create{Entity}Filler(dateTimeOffset, userId).Create();

        private static Filler<{Entity}> Create{Entity}Filler(
            DateTimeOffset dateTimeOffset,
            string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<{Entity}>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                // NOTE: Ignore navigation properties (e.g. .OnProperty(e => e.SomeNavProp).IgnoreIt())
                // NOTE: Override specific string properties with length constraints as needed.
                .OnProperty({entity} => {entity}.CreatedBy).Use(userId)
                .OnProperty({entity} => {entity}.UpdatedBy).Use(userId);

            return filler;
        }
    }
}

// {Entity}ServiceTests.Add.Logic.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldAdd{Entity}Async()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            {Entity} random{Entity} = Create{Entity}Filler(randomDateTimeOffset, randomUserId).Create();
            {Entity} input{Entity} = random{Entity};
            {Entity} auditApplied{Entity} = input{Entity}.DeepClone();
            auditApplied{Entity}.CreatedBy = randomUserId;
            auditApplied{Entity}.CreatedWhen = randomDateTimeOffset;
            auditApplied{Entity}.UpdatedBy = randomUserId;
            auditApplied{Entity}.UpdatedWhen = randomDateTimeOffset;
            {Entity} storage{Entity} = auditApplied{Entity}.DeepClone();
            {Entity} expected{Entity} = storage{Entity}.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(input{Entity}))
                    .ReturnsAsync(auditApplied{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Insert{Entity}Async(auditApplied{Entity}, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(storage{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Added"))
                    .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Add{Entity}Async(
                    input{Entity},
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                    broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.ApplyAddAuditValuesAsync(input{Entity}),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                    broker.Insert{Entity}Async(auditApplied{Entity}, It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                broker.Publish{Entity}Async(
                    It.IsAny<EventEnvelope<{Entity}>>(),
                    "{Entity}Added"),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.Add.Validations.cs
using System;
using System.Threading.Tasks;
using FluentAssertions;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIf{Entity}IsNullAndLogItAsync()
        {
            // given
            {Entity} null{Entity} = null;

            var null{Entity}Exception =
                new Null{Entity}Exception(message: "{Entity human-readable name} is null.");

            var expected{Entity}ValidationException =
                new {Entity}ValidationException(
                    message: "{Entity human-readable name} validation error occurred, fix the errors and try again.",
                    innerException: null{Entity}Exception);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(null{Entity}))
                    .ReturnsAsync(null{Entity});

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    null{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}ValidationException actual{Entity}ValidationException =
                await Assert.ThrowsAsync<{Entity}ValidationException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}ValidationException.Should().BeEquivalentTo(
                expected{Entity}ValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(null{Entity}),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnAddIf{Entity}IsInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            var invalid{Entity} = new {Entity}
            {
                Id = Guid.Empty,
                // NOTE: Set entity-specific required fields to Guid.Empty / invalidText / default here.
                CreatedBy = invalidText,
                UpdatedBy = invalidText,
                CreatedWhen = default,
                UpdatedWhen = default
            };

            var invalid{Entity}Exception =
                new Invalid{Entity}Exception(
                    message: "{Entity human-readable name} is invalid, fix the errors and try again.");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.Id),
                values: "Id is required");

            // NOTE: Add AddData calls for all required entity-specific fields here.

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.CreatedBy),
                values: "Text is required");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.UpdatedBy),
                values: "Text is required");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.CreatedWhen),
                values: "Date is required");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.UpdatedWhen),
                values: "Date is required");

            var expected{Entity}ValidationException =
                new {Entity}ValidationException(
                    message: "{Entity human-readable name} validation error occurred, fix the errors and try again.",
                    innerException: invalid{Entity}Exception);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}))
                    .ReturnsAsync(invalid{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(invalidText);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(default(DateTimeOffset));

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    invalid{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}ValidationException actual{Entity}ValidationException =
                await Assert.ThrowsAsync<{Entity}ValidationException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}ValidationException.Should().BeEquivalentTo(
                expected{Entity}ValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfUpdatedWhenIsNotSameAsCreatedWhenAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            {Entity} random{Entity} = Create{Entity}Filler(randomDateTimeOffset, randomUserId).Create();
            {Entity} invalid{Entity} = random{Entity};
            invalid{Entity}.UpdatedWhen = GetRandomDateTimeOffset();

            var invalid{Entity}Exception =
                new Invalid{Entity}Exception(
                    message: "{Entity human-readable name} is invalid, fix the errors and try again.");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.UpdatedWhen),
                values: $"Date is not the same as {nameof({Entity}.CreatedWhen)}");

            var expected{Entity}ValidationException =
                new {Entity}ValidationException(
                    message: "{Entity human-readable name} validation error occurred, fix the errors and try again.",
                    innerException: invalid{Entity}Exception);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}))
                    .ReturnsAsync(invalid{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    invalid{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}ValidationException actual{Entity}ValidationException =
                await Assert.ThrowsAsync<{Entity}ValidationException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}ValidationException.Should().BeEquivalentTo(
                expected{Entity}ValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIf{Entity}ExceedsMaxLengthAndLogItAsync()
        {
            // given
            string randomUserId = GetRandomStringWithLengthOf(256);
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            {Entity} invalid{Entity} = Create{Entity}Filler(randomDateTimeOffset, randomUserId).Create();

            var invalid{Entity}Exception =
                new Invalid{Entity}Exception(
                    message: "{Entity human-readable name} is invalid, fix the errors and try again.");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.CreatedBy),
                values: $"Text exceed max length of {invalid{Entity}.CreatedBy.Length - 1} characters");

            invalid{Entity}Exception.AddData(
                key: nameof({Entity}.UpdatedBy),
                values: $"Text exceed max length of {invalid{Entity}.UpdatedBy.Length - 1} characters");

            var expected{Entity}ValidationException =
                new {Entity}ValidationException(
                    message: "{Entity human-readable name} validation error occurred, fix the errors and try again.",
                    innerException: invalid{Entity}Exception);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}))
                    .ReturnsAsync(invalid{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    invalid{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}ValidationException actual{Entity}ValidationException =
                await Assert.ThrowsAsync<{Entity}ValidationException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}ValidationException.Should().BeEquivalentTo(
                expected{Entity}ValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(invalid{Entity}),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.Add.Exceptions.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using EFxceptions.Models.Exceptions;
using FluentAssertions;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xeptions;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnAddIfErrorOccursAndLogItAsync(
            Exception thrownException,
            Xeption expectedInnerException)
        {
            // given
            {Entity} some{Entity} = CreateRandom{Entity}();

            var expected{Entity}DependencyException = new {Entity}DependencyException(
                message: "{Entity human-readable name} dependency error occurred, contact support.",
                innerException: expectedInnerException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}))
                    .ThrowsAsync(thrownException);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    some{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}DependencyException actual{Entity}DependencyException =
                await Assert.ThrowsAsync<{Entity}DependencyException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}DependencyException.Should().BeEquivalentTo(
                expected{Entity}DependencyException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}DependencyException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledExceptionOnAddIfCancellationRequestedAndLogItAsync()
        {
            // given
            {Entity} some{Entity} = CreateRandom{Entity}();
            var cancellationToken = new CancellationToken(canceled: true);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    some{Entity},
                    cancellationToken);

            // then
            await Assert.ThrowsAsync<OperationCanceledException>(
                add{Entity}Task.AsTask);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfDependencyValidationErrorOccursAndLogItAsync(
            Exception thrownException,
            Xeption expectedInnerException)
        {
            // given
            {Entity} some{Entity} = CreateRandom{Entity}();

            var expected{Entity}DependencyValidationException = new {Entity}DependencyValidationException(
                message: "{Entity human-readable name} dependency validation error occurred, fix the errors and try again.",
                innerException: expectedInnerException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}))
                    .ThrowsAsync(thrownException);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    some{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}DependencyValidationException actual{Entity}DependencyValidationException =
                await Assert.ThrowsAsync<{Entity}DependencyValidationException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}DependencyValidationException.Should().BeEquivalentTo(
                expected{Entity}DependencyValidationException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}DependencyValidationException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            {Entity} some{Entity} = CreateRandom{Entity}();
            var serviceException = new Exception();

            var failed{Entity}ServiceException = new Failed{Entity}ServiceException(
                message: "Failed {entity} service error occurred, please contact support.",
                innerException: serviceException,
                data: serviceException.Data);

            var expected{Entity}ServiceException = new {Entity}ServiceException(
                message: "{Entity human-readable name} service error occurred, contact support.",
                innerException: failed{Entity}ServiceException);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<{Entity}> add{Entity}Task =
                this.{entity}Service.Add{Entity}Async(
                    some{Entity},
                    TestContext.Current.CancellationToken);

            {Entity}ServiceException actual{Entity}ServiceException =
                await Assert.ThrowsAsync<{Entity}ServiceException>(
                    add{Entity}Task.AsTask);

            // then
            actual{Entity}ServiceException.Should().BeEquivalentTo(
                expected{Entity}ServiceException);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(some{Entity}),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ServiceException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.RetrieveAll.Logic.cs
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldRetrieveAll{Entity}sAsync()
        {
            // given
            IQueryable<{Entity}> random{Entity}s = CreateRandom{Entity}s();
            IQueryable<{Entity}> storage{Entity}s = random{Entity}s;
            IQueryable<{Entity}> expected{Entity}s = storage{Entity}s;

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAll{Entity}sAsync())
                    .ReturnsAsync(storage{Entity}s);

            // when
            IQueryable<{Entity}> actual{Entity}s =
                await this.{entity}Service.RetrieveAll{Entity}sAsync(
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}s.Should().BeEquivalentTo(expected{Entity}s);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAll{Entity}sAsync(),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.RetrieveAll.Exceptions.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using {Namespace}.Models.Foundations.{Entity}s;
using {Namespace}.Models.Foundations.{Entity}s.Exceptions;
using Microsoft.Data.SqlClient;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnRetrieveAllIfOperationCanceledExceptionOccursAndLogItAsync()
        {
            // given
            var operationCanceledException = new OperationCanceledException();

            var timeout{Entity}Exception = new Timeout{Entity}Exception(
                message: "{Entity human-readable name} timed out, contact support.",
                innerException: new TimeoutException(),
                data: operationCanceledException.Data);

            var expected{Entity}DependencyException = new {Entity}DependencyException(
                message: "{Entity human-readable name} dependency error occurred, contact support.",
                innerException: timeout{Entity}Exception);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAll{Entity}sAsync())
                    .ThrowsAsync(operationCanceledException);

            // when
            ValueTask<IQueryable<{Entity}>> retrieveAll{Entity}sTask =
                this.{entity}Service.RetrieveAll{Entity}sAsync(
                    TestContext.Current.CancellationToken);

            {Entity}DependencyException actual{Entity}DependencyException =
                await Assert.ThrowsAsync<{Entity}DependencyException>(
                    retrieveAll{Entity}sTask.AsTask);

            // then
            actual{Entity}DependencyException.Should().BeEquivalentTo(
                expected{Entity}DependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAll{Entity}sAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}DependencyException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowOperationCanceledExceptionOnRetrieveAllIfCancellationRequestedAndLogItAsync()
        {
            // given
            var cancellationToken = new CancellationToken(canceled: true);

            // when
            ValueTask<IQueryable<{Entity}>> retrieveAll{Entity}sTask =
                this.{entity}Service.RetrieveAll{Entity}sAsync(cancellationToken);

            // then
            await Assert.ThrowsAsync<OperationCanceledException>(
                retrieveAll{Entity}sTask.AsTask);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnRetrieveAllIfSqlErrorOccursAndLogItAsync()
        {
            // given
            SqlException sqlException = GetSqlException();

            var failedStorage{Entity}Exception = new FailedStorage{Entity}Exception(
                message: "Failed {entity} storage error occurred, contact support.",
                innerException: sqlException,
                data: sqlException.Data);

            var expected{Entity}DependencyException = new {Entity}DependencyException(
                message: "{Entity human-readable name} dependency error occurred, contact support.",
                innerException: failedStorage{Entity}Exception);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAll{Entity}sAsync())
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<IQueryable<{Entity}>> retrieveAll{Entity}sTask =
                this.{entity}Service.RetrieveAll{Entity}sAsync(
                    TestContext.Current.CancellationToken);

            {Entity}DependencyException actual{Entity}DependencyException =
                await Assert.ThrowsAsync<{Entity}DependencyException>(
                    retrieveAll{Entity}sTask.AsTask);

            // then
            actual{Entity}DependencyException.Should().BeEquivalentTo(
                expected{Entity}DependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAll{Entity}sAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(
                    SameExceptionAs(expected{Entity}DependencyException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnRetrieveAllIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var serviceException = new Exception();

            var failed{Entity}ServiceException = new Failed{Entity}ServiceException(
                message: "Failed {entity} service error occurred, please contact support.",
                innerException: serviceException,
                data: serviceException.Data);

            var expected{Entity}ServiceException = new {Entity}ServiceException(
                message: "{Entity human-readable name} service error occurred, contact support.",
                innerException: failed{Entity}ServiceException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectAll{Entity}sAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<IQueryable<{Entity}>> retrieveAll{Entity}sTask =
                this.{entity}Service.RetrieveAll{Entity}sAsync(
                    TestContext.Current.CancellationToken);

            {Entity}ServiceException actual{Entity}ServiceException =
                await Assert.ThrowsAsync<{Entity}ServiceException>(
                    retrieveAll{Entity}sTask.AsTask);

            // then
            actual{Entity}ServiceException.Should().BeEquivalentTo(
                expected{Entity}ServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectAll{Entity}sAsync(),
                Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(
                    SameExceptionAs(expected{Entity}ServiceException))),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.RetrieveById.Logic.cs
using System.Threading.Tasks;
using FluentAssertions;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldRetrieve{Entity}ByIdAsync()
        {
            // given
            {Entity} random{Entity} = CreateRandom{Entity}();
            {Entity} storage{Entity} = random{Entity};
            {Entity} expected{Entity} = storage{Entity};

            this.storageBrokerMock.Setup(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    TestContext.Current.CancellationToken))
                        .ReturnsAsync(storage{Entity});

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Retrieve{Entity}ByIdAsync(
                    random{Entity}.Id,
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.storageBrokerMock.Verify(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    TestContext.Current.CancellationToken),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.RetrieveById.Validations.cs — validates empty Guid and not-found
// {Entity}ServiceTests.RetrieveById.Exceptions.cs — follows same pattern as Add.Exceptions

// {Entity}ServiceTests.Modify.Logic.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldModify{Entity}Async()
        {
            // given
            string randomUserId = GetRandomString();
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            {Entity} random{Entity} = CreateRandomModify{Entity}(randomDateTimeOffset, randomUserId);
            {Entity} input{Entity} = random{Entity};
            {Entity} auditApplied{Entity} = input{Entity}.DeepClone();
            {Entity} storage{Entity} = auditApplied{Entity}.DeepClone();
            storage{Entity}.UpdatedWhen = storage{Entity}.UpdatedWhen.AddDays(GetRandomNegativeNumber());
            {Entity} auditPreserved{Entity} = auditApplied{Entity}.DeepClone();
            {Entity} updated{Entity} = auditPreserved{Entity}.DeepClone();
            {Entity} expected{Entity} = updated{Entity}.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.GetUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyModifyAuditValuesAsync(input{Entity}))
                    .ReturnsAsync(auditApplied{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Select{Entity}ByIdAsync(
                    auditApplied{Entity}.Id,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storage{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(
                    auditApplied{Entity},
                    storage{Entity}))
                        .ReturnsAsync(auditPreserved{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Update{Entity}Async(auditPreserved{Entity}, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updated{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Modified"))
                    .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Modify{Entity}Async(
                    input{Entity},
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.GetUserIdAsync(),
                Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                    broker.GetCurrentDateTimeOffsetAsync(),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.ApplyModifyAuditValuesAsync(input{Entity}),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                    broker.Select{Entity}ByIdAsync(
                        auditApplied{Entity}.Id,
                        It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                    broker.EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(
                        auditApplied{Entity},
                        storage{Entity}),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                    broker.Update{Entity}Async(auditPreserved{Entity}, It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                    broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Modified"),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.Modify.Validations.cs — validates all Modify-specific rules
// {Entity}ServiceTests.Modify.Exceptions.cs — uses ModifyDependencyValidationExceptions TheoryData

// {Entity}ServiceTests.RemoveById.Logic.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldRemove{Entity}ByIdAsync()
        {
            // given
            {Entity} random{Entity} = CreateRandom{Entity}();
            random{Entity}.IsDeleted = false;
            {Entity} storage{Entity} = random{Entity};

            {Entity} audited{Entity} = storage{Entity}.DeepClone();
            audited{Entity}.IsDeleted = true;

            {Entity} expected{Entity} = audited{Entity}.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storage{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyRemoveAuditValuesAsync(storage{Entity}))
                    .ReturnsAsync(audited{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Update{Entity}Async(audited{Entity}, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"))
                    .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Remove{Entity}ByIdAsync(
                    random{Entity}.Id,
                    deletionReason: null,
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.storageBrokerMock.Verify(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyRemoveAuditValuesAsync(storage{Entity}),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.Update{Entity}Async(audited{Entity}, It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldRemove{Entity}ByIdWithDeletionReasonAsync()
        {
            // given
            string someDeletionReason = GetRandomString();
            {Entity} random{Entity} = CreateRandom{Entity}();
            random{Entity}.IsDeleted = false;
            {Entity} storage{Entity} = random{Entity};

            {Entity} audited{Entity} = storage{Entity}.DeepClone();
            audited{Entity}.IsDeleted = true;
            audited{Entity}.DeletionReason = someDeletionReason;

            {Entity} expected{Entity} = audited{Entity}.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storage{Entity});

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyRemoveAuditValuesAsync(storage{Entity}))
                    .ReturnsAsync(audited{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Update{Entity}Async(audited{Entity}, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"))
                    .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.Remove{Entity}ByIdAsync(
                    random{Entity}.Id,
                    deletionReason: someDeletionReason,
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.storageBrokerMock.Verify(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyRemoveAuditValuesAsync(storage{Entity}),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.Update{Entity}Async(audited{Entity}, It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.RemoveById.Validations.cs — validates empty Guid
// {Entity}ServiceTests.RemoveById.Exceptions.cs — uses DependencyExceptions + ModifyDependencyValidationExceptions

// {Entity}ServiceTests.HardRemoveById.Logic.cs
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using {Namespace}.Models.Events;
using {Namespace}.Models.Foundations.{Entity}s;
using Moq;

namespace {Namespace}.Tests.Unit.Services.Foundations.{Entity}s
{
    public partial class {Entity}ServiceTests
    {
        [Fact]
        public async Task ShouldHardRemove{Entity}ByIdAsync()
        {
            // given
            {Entity} random{Entity} = CreateRandom{Entity}();
            {Entity} storage{Entity} = random{Entity};
            {Entity} expected{Entity} = storage{Entity}.DeepClone();

            this.storageBrokerMock.Setup(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(storage{Entity});

            this.storageBrokerMock.Setup(broker =>
                broker.Delete{Entity}Async(storage{Entity}, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected{Entity});

            this.eventBrokerMock.Setup(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"))
                    .Returns(ValueTask.CompletedTask);

            // when
            {Entity} actual{Entity} =
                await this.{entity}Service.HardRemove{Entity}ByIdAsync(
                    random{Entity}.Id,
                    TestContext.Current.CancellationToken);

            // then
            actual{Entity}.Should().BeEquivalentTo(expected{Entity});

            this.storageBrokerMock.Verify(broker =>
                broker.Select{Entity}ByIdAsync(
                    random{Entity}.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.Delete{Entity}Async(storage{Entity}, It.IsAny<CancellationToken>()),
                Times.Once);

            this.eventBrokerMock.Verify(broker =>
                broker.Publish{Entity}Async(It.IsAny<EventEnvelope<{Entity}>>(), "{Entity}Removed"),
                Times.Once);

            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.eventBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}

// {Entity}ServiceTests.HardRemoveById.Validations.cs — validates empty Guid
// {Entity}ServiceTests.HardRemoveById.Exceptions.cs — uses DependencyExceptions + ModifyDependencyValidationExceptions

// ─────────────────────────────────────────────────────────────────────────────
// KEY PATTERNS & RULES (apply across all test files)
// ─────────────────────────────────────────────────────────────────────────────
//
// 1. CANCELLATION TESTS: Every operation must have a test for:
//    - OperationCanceledException when IsCancellationRequested is FALSE (→ Timeout dependency)
//    - OperationCanceledException when IsCancellationRequested is TRUE  (→ rethrow, no mocks called)
//
// 2. THEORY DATA METHODS (in base test class):
//    - DependencyExceptions(): OperationCanceledException → TimeoutXxxException,
//                              DbUpdateException → FailedStorageXxxException
//    - DependencyValidationExceptions(): DuplicateKeyException → AlreadyExistsXxxException,
//                                        ForeignKeyConstraintConflictException → InvalidXxxReferenceException
//    - ModifyDependencyValidationExceptions(): DbUpdateConcurrencyException → LockedXxxException,
//                                              ForeignKeyConstraintConflictException → InvalidXxxReferenceException
//
// 3. AUDIT FIELD NAMES: Use CreatedWhen/UpdatedWhen (not CreatedDate/UpdatedDate).
//
// 4. FILLER SETUP: Ignore navigation properties. Override CreatedBy/UpdatedBy with userId.
//    String length properties that have DB constraints (e.g. max 255) should use
//    GetRandomStringWithLengthOf(255) to keep values within bounds.
//
// 5. EVENT BROKER MOCK: Use It.IsAny<EventEnvelope<{Entity}>>() — do NOT match the envelope
//    by reference since it is constructed inside the service method.
//
// 6. STORAGE BROKER MOCK for SelectByIdAsync: Use It.IsAny<CancellationToken>() in logic
//    tests; use TestContext.Current.CancellationToken in validation/exception tests where
//    the exact token flows through.
//
// 7. ALL TEST METHODS use TestContext.Current.CancellationToken as the cancellation token
//    argument, except cancellation-specific tests that pass `new CancellationToken(canceled: true)`.
//
// 8. TEST FILE STRUCTURE (one partial class per concern per operation):
//    Logic | Validations | Exceptions  for each of:
//    Add, RetrieveAll, RetrieveById, Modify, RemoveById, HardRemoveById
