using BusinessEntity = Agendify.Models.Entities.Business;
using Agendify.Models.Entities;
using Agendify.DTOs.Auth;
using Agendify.Data;
using Microsoft.EntityFrameworkCore;

namespace Agendify.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AgendifyDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(AgendifyDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el email ya existe
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Crear el Business primero
        var business = new BusinessEntity
        {
            Name = registerDto.BusinessName,
            Industry = registerDto.Industry
        };

        _context.Businesses.Add(business);
        await _context.SaveChangesAsync();

        // Crear el Provider (el usuario será el primer proveedor del negocio)
        var provider = new Provider
        {
            BusinessId = business.Id,
            Name = registerDto.ProviderName,
            Specialty = registerDto.ProviderSpecialty,
            IsActive = true
        };

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Crear el User y asociarlo con el Provider
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            BusinessId = business.Id,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generar token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId,
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");
        }

        // Generar token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            BusinessId = user.BusinessId
        };
    }
}

