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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, message) = GetExceptionDetails(exception);
        var problemDetails = CreateProblemDetails(context, statusCode, title, message, exception);
        await WriteResponseAsync(context, problemDetails, statusCode);
    }

    /// <summary>
    /// Determina el código de estado, título y mensaje según el tipo de excepción.
    /// </summary>
    private (HttpStatusCode statusCode, string title, string message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            // Errores de base de datos o servicios externos
            InvalidOperationException when exception.Source?.Contains("EntityFramework") == true
                => (HttpStatusCode.ServiceUnavailable, 
                    "Service Unavailable", 
                    "The service is temporarily unavailable. Please try again later."),
            
            TimeoutException 
                => (HttpStatusCode.ServiceUnavailable, 
                    "Service Unavailable", 
                    "The service is temporarily unavailable. Please try again later."),
            
            // Errores de validación de argumentos (bugs de programación)
            ArgumentNullException or ArgumentException 
                => (HttpStatusCode.InternalServerError, 
                    "Internal Server Error", 
                    "An internal error occurred."),
            
            // Default: Error interno del servidor no categorizado
            _ => (HttpStatusCode.InternalServerError, 
                  "Internal Server Error", 
                  "An unexpected error occurred. Please try again later.")
        };
    }

    /// <summary>
    /// Crea el objeto ProblemDetails según RFC 7807.
    /// </summary>
    private ProblemDetails CreateProblemDetails(
        HttpContext context, 
        HttpStatusCode statusCode, 
        string title, 
        string message, 
        Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = message,
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Instance = context.Request.Path
        };

        // En desarrollo, incluir stack trace completo
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.ToString();
        }

        return problemDetails;
    }

    /// <summary>
    /// Serializa y escribe la respuesta en el HttpContext.
    /// </summary>
    private async Task WriteResponseAsync(HttpContext context, ProblemDetails problemDetails, HttpStatusCode statusCode)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = _environment.IsDevelopment(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = JsonSerializer.Serialize(problemDetails, jsonOptions);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        
        await context.Response.WriteAsync(result);
    }
}

