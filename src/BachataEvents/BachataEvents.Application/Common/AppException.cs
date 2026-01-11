namespace BachataEvents.Application.Common;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class ValidationFailedException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}
