﻿﻿using BusinessEntity = Agendify.Models.Entities.Business;
using Agendify.Models.Entities;
using Agendify.DTOs.Auth;
using Agendify.Common.Errors;
using Agendify.Repositories;
using Agendify.Services.Auth.Password;
using Agendify.Services.Auth.JWT;
using Agendify.Services.ProviderSchedules;
using FluentResults;

namespace Agendify.Services.Auth.Authentication;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<BusinessEntity> _businessRepository;
    private readonly IRepository<Provider> _providerRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IProviderScheduleService _providerScheduleService;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<BusinessEntity> businessRepository,
        IRepository<Provider> providerRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IProviderScheduleService providerScheduleService)
    {
        _userRepository = userRepository;
        _businessRepository = businessRepository;
        _providerRepository = providerRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _providerScheduleService = providerScheduleService;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el email ya existe
        var existingUser = await _userRepository.ExistsAsync(u => u.Email == registerDto.Email);
        if (existingUser)
        {
            return Result.Fail(new ConflictError("El email ya está registrado"));
        }

        // 1. Crear el Business primero
        var business = new BusinessEntity
        {
            Name = registerDto.BusinessName,
            Industry = registerDto.Industry
        };
        business = await _businessRepository.AddAsync(business);

        // 2. Crear el Provider (el usuario será el primer proveedor del negocio)
        var provider = new Provider
        {
            BusinessId = business.Id,
            Name = registerDto.ProviderName,
            Specialty = registerDto.ProviderSpecialty,
            IsActive = true
        };
        provider = await _providerRepository.AddAsync(provider);

        // 3. Crear el User con el ProviderId ya asignado
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            BusinessId = business.Id,
            ProviderId = provider.Id
        };
        user = await _userRepository.AddAsync(user);

        // 4. Crear horarios por defecto (lunes a viernes de 09:00 a 18:00)
        await _providerScheduleService.CreateDefaultSchedulesAsync(provider.Id);

        // 5. Generar par de tokens
        var tokenPair = _jwtService.GenerateTokenPair(user);
        
        // 6. Guardar el refresh token en la tabla RefreshTokens
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenPair.RefreshToken,
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Ok(new AuthResponseDto
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId
        });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por email
        var users = await _userRepository.FindAsync(u => u.Email == loginDto.Email);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return Result.Fail(new UnauthorizedError("Email o contraseña incorrectos"));
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Result.Fail(new UnauthorizedError("Email o contraseña incorrectos"));
        }

        // Generar par de tokens
        var tokenPair = _jwtService.GenerateTokenPair(user);
        
        // Guardar el nuevo refresh token en la tabla RefreshTokens
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenPair.RefreshToken,
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Ok(new AuthResponseDto
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId
        });
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        // 1. Validar el formato del refresh token
        var userId = _jwtService.ValidateRefreshToken(refreshTokenDto.RefreshToken);
        if (userId == null)
        {
            return Result.Fail(new UnauthorizedError("Refresh token inválido o expirado"));
        }

        // 2. Buscar el refresh token en la base de datos
        var storedTokens = await _refreshTokenRepository.FindAsync(rt => 
            rt.UserId == userId.Value && 
            rt.Token == refreshTokenDto.RefreshToken);
        
        var storedToken = storedTokens.FirstOrDefault();
        
        if (storedToken == null)
        {
            return Result.Fail(new UnauthorizedError("Refresh token no encontrado"));
        }

        // 3. Verificar que el token no esté revocado
        if (storedToken.IsRevoked)
        {
            return Result.Fail(new UnauthorizedError("Refresh token revocado"));
        }

        // 4. Verificar que no haya expirado
        if (storedToken.IsExpired)
        {
            return Result.Fail(new UnauthorizedError("Refresh token expirado"));
        }

        // 5. Buscar el usuario
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return Result.Fail(new UnauthorizedError("Usuario no encontrado"));
        }

        // 6. Revocar el refresh token antiguo (rotación de tokens)
        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        // 7. Generar nuevo par de tokens
        var tokenPair = _jwtService.GenerateTokenPair(user);
        
        // 8. Guardar el nuevo refresh token
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenPair.RefreshToken,
            ExpiresAt = tokenPair.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        return Result.Ok(new AuthResponseDto
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAt = tokenPair.AccessTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId
        });
    }
}

