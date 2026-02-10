namespace Agendify.DTOs.Auth;

/// <summary>
/// DTO interno para representar un par de tokens (access + refresh)
/// </summary>
public class TokenPairDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

