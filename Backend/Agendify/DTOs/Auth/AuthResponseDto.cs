﻿namespace Agendify.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public int ProviderId { get; set; }
}

