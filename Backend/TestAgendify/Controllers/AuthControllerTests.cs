using Agendify.Common.Errors;
using Agendify.Controllers;
using Agendify.DTOs.Auth;
using Agendify.Services.Auth.Authentication;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestAgendify.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _sut = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedWithAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            BusinessName = "Test Business",
            Industry = "Technology"
        };
        
        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "test.access.token",
            RefreshToken = "test.refresh.token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = 1,
            Email = registerDto.Email,
            BusinessId = 1
        };
        
        _mockAuthService
            .Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.ActionName.Should().Be(nameof(_sut.Register));
        
        _mockAuthService.Verify(
            s => s.RegisterAsync(registerDto),
            Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            BusinessName = "Test Business",
            Industry = "Technology"
        };
        
        _mockAuthService
            .Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync(Result.Fail("El email ya está registrado"));

        // Act
        var result = await _sut.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockAuthService.Verify(
            s => s.RegisterAsync(registerDto),
            Times.Once);
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "123", // Too short
            BusinessName = "",
            Industry = ""
        };
        
        _mockAuthService
            .Setup(s => s.RegisterAsync(registerDto))
            .ReturnsAsync(Result.Fail("Datos de registro inválidos"));

        // Act
        var result = await _sut.Register(registerDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockAuthService.Verify(
            s => s.RegisterAsync(registerDto),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        
        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "test.access.token",
            RefreshToken = "test.refresh.token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = 1,
            Email = loginDto.Email,
            BusinessId = 1
        };
        
        _mockAuthService
            .Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockAuthService.Verify(
            s => s.LoginAsync(loginDto),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };
        
        _mockAuthService
            .Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Credenciales inválidas")));

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        
        _mockAuthService.Verify(
            s => s.LoginAsync(loginDto),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };
        
        _mockAuthService
            .Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Usuario no encontrado")));

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        
        _mockAuthService.Verify(
            s => s.LoginAsync(loginDto),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "",
            Password = "Password123!"
        };
        
        _mockAuthService
            .Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(Result.Fail("Email es requerido"));

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockAuthService.Verify(
            s => s.LoginAsync(loginDto),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = ""
        };
        
        _mockAuthService
            .Setup(s => s.LoginAsync(loginDto))
            .ReturnsAsync(Result.Fail(new BadRequestError("Password es requerido")));

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockAuthService.Verify(
            s => s.LoginAsync(loginDto),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewTokens()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid.refresh.token"
        };
        
        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "new.access.token",
            RefreshToken = "new.refresh.token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = 1,
            Email = "test@example.com",
            BusinessId = 1
        };
        
        _mockAuthService
            .Setup(s => s.RefreshTokenAsync(refreshTokenDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.RefreshToken(refreshTokenDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockAuthService.Verify(
            s => s.RefreshTokenAsync(refreshTokenDto),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "invalid.refresh.token"
        };
        
        _mockAuthService
            .Setup(s => s.RefreshTokenAsync(refreshTokenDto))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Refresh token inválido o expirado")));

        // Act
        var result = await _sut.RefreshToken(refreshTokenDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        
        _mockAuthService.Verify(
            s => s.RefreshTokenAsync(refreshTokenDto),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "expired.refresh.token"
        };
        
        _mockAuthService
            .Setup(s => s.RefreshTokenAsync(refreshTokenDto))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Refresh token expirado")));

        // Act
        var result = await _sut.RefreshToken(refreshTokenDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        
        _mockAuthService.Verify(
            s => s.RefreshTokenAsync(refreshTokenDto),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithMismatchedToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "mismatched.refresh.token"
        };
        
        _mockAuthService
            .Setup(s => s.RefreshTokenAsync(refreshTokenDto))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Refresh token no coincide")));

        // Act
        var result = await _sut.RefreshToken(refreshTokenDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        
        _mockAuthService.Verify(
            s => s.RefreshTokenAsync(refreshTokenDto),
            Times.Once);
    }
}

