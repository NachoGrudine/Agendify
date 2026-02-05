﻿﻿using Agendify.DTOs.Auth;
using Agendify.Services.Auth.Authentication;
using Agendify.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Authentication and authorization controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("User registration and authentication endpoints (login/logout)")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user, business, and first provider
    /// </summary>
    /// <param name="registerDto">Registration data (email, password, business_name, industry, provider_name, provider_specialty)</param>
    /// <returns>JWT token and user data</returns>
    /// <response code="201">Successfully registered</response>
    /// <response code="400">Invalid data or email already exists</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register new user",
        Description = "Creates a new account with a business and its first provider. Returns JWT token.",
        OperationId = "Register",
        Tags = new[] { "Auth" }
    )]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return result.ToCreatedResult(nameof(Register), x => new { userId = x.UserId });
    }

    /// <summary>
    /// Authenticates a user and generates JWT token
    /// </summary>
    /// <remarks>
    /// Validates credentials and returns a JWT token valid for 7 days.
    /// Include the token in header: Authorization Bearer {token}
    /// </remarks>
    /// <param name="loginDto">Credentials (email, password)</param>
    /// <returns>JWT token and user data</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Authenticates credentials and returns a JWT token valid for 7 days.",
        OperationId = "Login",
        Tags = new[] { "Auth" }
    )]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        // Login no crea recurso → 200 OK o 401 Unauthorized
        return result.ToActionResult();
    }
}

