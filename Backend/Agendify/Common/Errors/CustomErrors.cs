using FluentResults;

namespace Agendify.Common.Errors;

/// <summary>
/// Errores personalizados que determinan el código HTTP de respuesta
/// </summary>
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
    }
}

public class ValidationError : Error
{
    public ValidationError(string message) : base(message)
    {
    }
}

public class BadRequestError : Error
{
    public BadRequestError(string message) : base(message)
    {
    }
}

public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message)
    {
    }
}

public class ConflictError : Error
{
    public ConflictError(string message) : base(message)
    {
    }
}

public class ForbiddenError : Error
{
    public ForbiddenError(string message) : base(message)
    {
    }
}

