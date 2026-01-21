﻿using Agendify.API.DTOs.Auth;

namespace Agendify.API.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}

