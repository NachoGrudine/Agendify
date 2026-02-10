﻿using Agendify.DTOs.Auth;
using Agendify.Models.Entities;

namespace Agendify.Services.Auth.JWT;

public interface IJwtService
{
    /// <summary>
    /// Genera un par de tokens (Access Token + Refresh Token) para un usuario
    /// </summary>
    TokenPairDto GenerateTokenPair(User user);
    
    /// <summary>
    /// Valida un Access Token y retorna el UserId si es válido
    /// </summary>
    int? ValidateAccessToken(string token);
    
    /// <summary>
    /// Valida un Refresh Token y retorna el UserId si es válido
    /// </summary>
    int? ValidateRefreshToken(string token);
}

