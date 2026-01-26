using Agendify.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Convierte un Result<T> a una respuesta HTTP apropiada
    /// </summary>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return result.ToErrorResponse<T>();
    }

    /// <summary>
    /// Convierte un Result (sin valor) a una respuesta HTTP apropiada
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return result.ToErrorResponse();
    }
    /// <summary>
    /// Convierte un Result<T> a una respuesta CreatedAtAction extrayendo el ID del valor.
    /// Esta sobrecarga evita que el controller acceda a result.Value prematuramente.
    /// </summary>
    public static ActionResult<T> ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        Func<T, object> routeValuesSelector)
    {
        // Verificar primero si falló
        if (result.IsFailed)
        {
            return result.ToErrorResponse<T>();
        }

        // Solo si es exitoso, extraer el id usando el selector
        var routeValues = routeValuesSelector(result.Value);
        return new CreatedAtActionResult(actionName, null, routeValues, result.Value);
    }

    /// <summary>
    /// Convierte errores tipados a respuestas HTTP apropiadas usando ProblemDetails (RFC 7807)
    /// </summary>
    private static ActionResult<T> ToErrorResponse<T>(this ResultBase result)
    {
        var firstError = result.Errors.FirstOrDefault();
        var errorMessage = firstError?.Message ?? "Error desconocido";

        return firstError switch
        {
            NotFoundError => new NotFoundObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/404"
            }),
            
            ValidationError => new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/400"
            }),
            
            UnauthorizedError => new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/401"
            }),
            
            ConflictError => new ConflictObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/409"
            }),
            
            ForbiddenError => new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/403"
            })
            { StatusCode = StatusCodes.Status403Forbidden },
            
            _ => new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/400"
            })
        };
    }

    /// <summary>
    /// Convierte errores tipados a respuestas HTTP apropiadas (sin valor de retorno) usando ProblemDetails
    /// </summary>
    private static IActionResult ToErrorResponse(this ResultBase result)
    {
        var firstError = result.Errors.FirstOrDefault();
        var errorMessage = firstError?.Message ?? "Error desconocido";

        return firstError switch
        {
            NotFoundError => new NotFoundObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/404"
            }),
            
            ValidationError => new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/400"
            }),
            
            UnauthorizedError => new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/401"
            }),
            
            ConflictError => new ConflictObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/409"
            }),
            
            ForbiddenError => new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/403"
            })
            { StatusCode = StatusCodes.Status403Forbidden },
            
            _ => new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = errorMessage,
                Type = "https://httpstatuses.com/400"
            })
        };
    }
}

