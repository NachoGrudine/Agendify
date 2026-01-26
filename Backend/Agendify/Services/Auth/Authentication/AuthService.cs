using BusinessEntity = Agendify.Models.Entities.Business;
using Agendify.Models.Entities;
using Agendify.DTOs.Auth;
using Agendify.Common.Errors;
using Agendify.Repositories;
using Agendify.Services.Auth.Password;
using Agendify.Services.Auth.JWT;
using FluentResults;

namespace Agendify.Services.Auth.Authentication;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<BusinessEntity> _businessRepository;
    private readonly IRepository<Provider> _providerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<BusinessEntity> businessRepository,
        IRepository<Provider> providerRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _businessRepository = businessRepository;
        _providerRepository = providerRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el email ya existe
        var existingUser = await _userRepository.ExistsAsync(u => u.Email == registerDto.Email);
        if (existingUser)
        {
            return Result.Fail(new ConflictError("El email ya está registrado"));
        }

        // Crear el Business primero
        var business = new BusinessEntity
        {
            Name = registerDto.BusinessName,
            Industry = registerDto.Industry
        };

        business = await _businessRepository.AddAsync(business);

        // Crear el Provider (el usuario será el primer proveedor del negocio)
        var provider = new Provider
        {
            BusinessId = business.Id,
            Name = registerDto.ProviderName,
            Specialty = registerDto.ProviderSpecialty,
            IsActive = true
        };

        provider = await _providerRepository.AddAsync(provider);

        // Crear el User y asociarlo con el Provider
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            BusinessId = business.Id,
        };

        user = await _userRepository.AddAsync(user);

        // Generar token
        var token = _jwtService.GenerateToken(user);

        return Result.Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId,
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

        // Generar token
        var token = _jwtService.GenerateToken(user);

        return Result.Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId
        });
    }
}

