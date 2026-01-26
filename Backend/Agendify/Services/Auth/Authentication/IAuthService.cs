﻿using Agendify.DTOs.Auth;
using FluentResults;

namespace Agendify.Services.Auth.Authentication;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto);
}

