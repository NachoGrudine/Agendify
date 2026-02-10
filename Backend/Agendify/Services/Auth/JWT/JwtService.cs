﻿using Agendify.DTOs.Auth;
using Agendify.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Agendify.Services.Auth.JWT;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenPairDto GenerateTokenPair(User user)
    {
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes"));
        
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"));

        return new TokenPairDto
        {
            AccessToken = GenerateAccessToken(user, accessTokenExpiration),
            RefreshToken = GenerateRefreshToken(user, refreshTokenExpiration),
            AccessTokenExpiresAt = accessTokenExpiration,
            RefreshTokenExpiresAt = refreshTokenExpiration
        };
    }

    private string GenerateAccessToken(User user, DateTime expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Claims para el Access Token
        var claims = new[]
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Email", user.Email),
            new Claim("BusinessId", user.BusinessId.ToString()),
            new Claim("ProviderId", user.ProviderId.ToString()),
            new Claim("TokenType", "access") // Identificador del tipo de token
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user, DateTime expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Claims mínimos para el Refresh Token (solo UserId y tipo)
        var claims = new[]
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("TokenType", "refresh") // Identificador del tipo de token
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidateAccessToken(string token)
    {
        return ValidateToken(token, "access");
    }

    public int? ValidateRefreshToken(string token)
    {
        return ValidateToken(token, "refresh");
    }

    private int? ValidateToken(string token, string expectedTokenType)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            
            // Verificar que sea el tipo de token correcto
            var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "TokenType");
            if (tokenTypeClaim == null || tokenTypeClaim.Value != expectedTokenType)
            {
                return null;
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId");

            if (userIdClaim != null)
            {
                return int.Parse(userIdClaim.Value);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

