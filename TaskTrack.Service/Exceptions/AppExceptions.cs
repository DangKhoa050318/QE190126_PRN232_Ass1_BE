namespace TaskTrack.Service.Exceptions;

/// <summary>Requested resource does not exist (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Business rule violation, e.g. deleting a record that is still in use (HTTP 400).</summary>
public class BadRequestException(string message) : Exception(message);

/// <summary>Missing or wrong credentials (HTTP 401).</summary>
public class UnauthorizedException(string message) : Exception(message);

/// <summary>The resource already exists, e.g. an email that is already registered (HTTP 409).</summary>
public class ConflictException(string field, string message) : Exception(message)
{
    /// <summary>JSON name of the field that caused the conflict, reported in the "errors" dictionary.</summary>
    public string Field { get; } = field;
}

/// <summary>Field-level validation failure (HTTP 400 with an "errors" dictionary).</summary>
public class ValidationFailedException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationFailedException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }
}
