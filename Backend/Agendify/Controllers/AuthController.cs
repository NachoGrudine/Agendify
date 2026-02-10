﻿﻿﻿using Agendify.DTOs.Auth;
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
    /// <returns>Access token, refresh token and user data</returns>
    /// <response code="201">Successfully registered</response>
    /// <response code="400">Invalid data or email already exists</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register new user",
        Description = "Creates a new account with a business and its first provider. Returns access token (15 min) and refresh token (7 days).",
        OperationId = "Register",
        Tags = new[] { "Auth" }
    )]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return result.ToCreatedResult(nameof(Register), x => new { userId = x.UserId });
    }

    /// <summary>
    /// Authenticates a user and generates JWT tokens
    /// </summary>
    /// <remarks>
    /// Validates credentials and returns:
    /// - Access Token: Valid for 15 minutes, use in header: Authorization Bearer {access_token}
    /// - Refresh Token: Valid for 7 days, use to obtain new access tokens
    /// </remarks>
    /// <param name="loginDto">Credentials (email, password)</param>
    /// <returns>Access token, refresh token and user data</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Authenticates credentials and returns access token (15 min) and refresh token (7 days).",
        OperationId = "Login",
        Tags = new[] { "Auth" }
    )]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        // Login no crea recurso → 200 OK o 401 Unauthorized
        return result.ToActionResult();
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token
    /// </summary>
    /// <remarks>
    /// When the access token expires (15 minutes), use this endpoint with the refresh token 
    /// to obtain a new pair of tokens without requiring the user to login again.
    /// </remarks>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Tokens refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Generates new access and refresh tokens using a valid refresh token.",
        OperationId = "RefreshToken",
        Tags = new[] { "Auth" }
    )]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        return result.ToActionResult();
    }
}

