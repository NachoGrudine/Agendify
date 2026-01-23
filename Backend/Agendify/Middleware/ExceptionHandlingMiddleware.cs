using System.Net;
using System.Text.Json;

namespace Agendify.Middleware;

/// <summary>
/// Middleware para manejar SOLO excepciones no controladas (500x).
/// Las excepciones de flujo de negocio (404, 400, etc.) se manejan con Result Pattern.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Loguear la excepción con stack trace completo
            _logger.LogError(ex, 
                "Unhandled exception occurred. Path: {Path}, Method: {Method}", 
                context.Request.Path, 
                context.Request.Method);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";
        string? details = null;

        // Casos específicos de excepciones reales del sistema
        switch (exception)
        {
            // Errores de base de datos
            case InvalidOperationException when exception.Source?.Contains("EntityFramework") == true:
            case TimeoutException:
                statusCode = HttpStatusCode.ServiceUnavailable;
                message = "The service is temporarily unavailable. Please try again later.";
                _logger.LogError(exception, "Database or timeout error occurred");
                break;

            // Errores de autenticación/autorización del sistema (no de negocio)
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Authentication required.";
                break;

            // Errores de validación de argumentos (bugs de programación)
            case ArgumentNullException:
            case ArgumentException:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An internal error occurred.";
                _logger.LogError(exception, "Argument error - possible bug in code");
                break;

            // Default: Error interno del servidor
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        // En desarrollo, incluir detalles del error
        if (_environment.IsDevelopment())
        {
            details = exception.ToString();
        }

        var result = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode,
            details = details
        }, new JsonSerializerOptions
        {
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(result);
    }
}

