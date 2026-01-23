﻿namespace Agendify.DTOs.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderSpecialty { get; set; } = string.Empty;
}
