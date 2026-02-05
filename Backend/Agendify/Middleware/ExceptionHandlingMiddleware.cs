using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Middleware;

/// <summary>
/// Middleware para manejar SOLO excepciones no controladas del sistema (500x).
/// 
/// RESPONSABILIDAD:
/// - Captura excepciones reales de infraestructura (DB, timeouts, bugs de programación)
/// - Devuelve respuestas HTTP 500x usando ProblemDetails (RFC 7807)
/// - Loguea errores para diagnóstico
/// 
/// NO MANEJA:
/// - Errores de lógica de negocio (404, 400, 401, etc.) → Se manejan con Result Pattern
/// - Errores de validación → Se manejan con FluentValidation + ResultExtensions
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
        var title = "Internal Server Error";
        var message = "An unexpected error occurred. Please try again later.";
        string? stackTrace = null;

        // Casos específicos de excepciones reales del sistema
        switch (exception)
        {
            // Errores de base de datos o servicios externos
            case InvalidOperationException when exception.Source?.Contains("EntityFramework") == true:
            case TimeoutException:
                statusCode = HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                message = "The service is temporarily unavailable. Please try again later.";
                _logger.LogError(exception, "Database or timeout error occurred");
                break;

            // Errores de validación de argumentos (bugs de programación)
            case ArgumentNullException:
            case ArgumentException:
                statusCode = HttpStatusCode.InternalServerError;
                title = "Internal Server Error";
                message = "An internal error occurred.";
                _logger.LogError(exception, "Argument error - possible bug in code");
                break;

            // Default: Error interno del servidor no categorizado
            default:
                statusCode = HttpStatusCode.InternalServerError;
                title = "Internal Server Error";
                message = "An unexpected error occurred.";
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        // En desarrollo, incluir stack trace completo
        if (_environment.IsDevelopment())
        {
            stackTrace = exception.ToString();
        }

        // Usar ProblemDetails (RFC 7807) para consistencia con ResultExtensions
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = message,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Instance = context.Request.Path
        };

        // Agregar stack trace en desarrollo
        if (stackTrace != null)
        {
            problemDetails.Extensions["stackTrace"] = stackTrace;
        }

        var result = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(result);
    }
}

