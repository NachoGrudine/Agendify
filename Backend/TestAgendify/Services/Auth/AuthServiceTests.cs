using Agendify.Common.Errors;
using Agendify.DTOs.Auth;
using Agendify.Models.Entities;
using Agendify.Repositories;
using Agendify.Services.Auth.Authentication;
using Agendify.Services.Auth.JWT;
using Agendify.Services.Auth.Password;
using Agendify.Services.ProviderSchedules;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using BusinessEntity = Agendify.Models.Entities.Business;

namespace TestAgendify.Services.Auth;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<IRepository<BusinessEntity>> _mockBusinessRepository;
    private readonly Mock<IRepository<Provider>> _mockProviderRepository;
    private readonly Mock<IRepository<RefreshToken>> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IProviderScheduleService> _mockProviderScheduleService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockBusinessRepository = new Mock<IRepository<BusinessEntity>>();
        _mockProviderRepository = new Mock<IRepository<Provider>>();
        _mockRefreshTokenRepository = new Mock<IRepository<RefreshToken>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtService = new Mock<IJwtService>();
        _mockProviderScheduleService = new Mock<IProviderScheduleService>();

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockBusinessRepository.Object,
            _mockProviderRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtService.Object,
            _mockProviderScheduleService.Object
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_Should_Return_Fail_When_Email_Already_Exists()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        _mockUserRepository
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ConflictError>()
            .Which.Message.Should().Be("El email ya está registrado");
    }

    [Fact]
    public async Task RegisterAsync_Should_Create_Business_Provider_User_And_Return_AuthResponse_When_Successful()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = "Developer"
        };

        var createdBusiness = new BusinessEntity
        {
            Id = 1,
            Name = registerDto.BusinessName,
            Industry = registerDto.Industry
        };

        var createdProvider = new Provider
        {
            Id = 1,
            BusinessId = createdBusiness.Id,
            Name = registerDto.ProviderName,
            Specialty = registerDto.ProviderSpecialty,
            IsActive = true
        };

        var createdUser = new User
        {
            Id = 1,
            Email = registerDto.Email,
            PasswordHash = "hashedpassword",
            BusinessId = createdBusiness.Id,
            ProviderId = createdProvider.Id
        };

        _mockUserRepository
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockBusinessRepository
            .Setup(x => x.AddAsync(It.IsAny<BusinessEntity>()))
            .ReturnsAsync(createdBusiness);

        _mockProviderRepository
            .Setup(x => x.AddAsync(It.IsAny<Provider>()))
            .ReturnsAsync(createdProvider);

        _mockPasswordHasher
            .Setup(x => x.HashPassword(registerDto.Password))
            .Returns("hashedpassword");

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _mockProviderScheduleService
            .Setup(x => x.CreateDefaultSchedulesAsync(createdProvider.Id))
            .ReturnsAsync(FluentResults.Result.Ok());

        var tokenPair = new TokenPairDto
        {
            AccessToken = "generated-access-token",
            RefreshToken = "generated-refresh-token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(It.IsAny<User>()))
            .Returns(tokenPair);

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken 
            { 
                Id = 1, 
                UserId = createdUser.Id, 
                Token = tokenPair.RefreshToken,
                ExpiresAt = tokenPair.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("generated-access-token");
        result.Value.RefreshToken.Should().Be("generated-refresh-token");
        result.Value.UserId.Should().Be(createdUser.Id);
        result.Value.Email.Should().Be(createdUser.Email);
        result.Value.BusinessId.Should().Be(1);

        // Verify all services were called correctly
        _mockUserRepository.Verify(x => x.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _mockBusinessRepository.Verify(x => x.AddAsync(It.Is<BusinessEntity>(b =>
            b.Name == registerDto.BusinessName && b.Industry == registerDto.Industry)), Times.Once);
        _mockProviderRepository.Verify(x => x.AddAsync(It.Is<Provider>(p =>
            p.Name == registerDto.ProviderName && p.Specialty == registerDto.ProviderSpecialty)), Times.Once);
        _mockPasswordHasher.Verify(x => x.HashPassword(registerDto.Password), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockProviderScheduleService.Verify(x => x.CreateDefaultSchedulesAsync(createdProvider.Id), Times.Once);
        _mockJwtService.Verify(x => x.GenerateTokenPair(It.IsAny<User>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == createdUser.Id && 
            rt.Token == tokenPair.RefreshToken && 
            rt.ExpiresAt == tokenPair.RefreshTokenExpiresAt)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Hash_Password_Before_Saving()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "plainpassword",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var hashedPassword = "hashed_plainpassword";

        _mockUserRepository
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(false);

        _mockBusinessRepository
            .Setup(x => x.AddAsync(It.IsAny<BusinessEntity>()))
            .ReturnsAsync(new BusinessEntity { Id = 1 });

        _mockProviderRepository
            .Setup(x => x.AddAsync(It.IsAny<Provider>()))
            .ReturnsAsync(new Provider { Id = 1, BusinessId = 1 });

        _mockPasswordHasher
            .Setup(x => x.HashPassword(registerDto.Password))
            .Returns(hashedPassword);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = 1, Email = registerDto.Email, PasswordHash = hashedPassword, BusinessId = 1, ProviderId = 1 });

        _mockProviderScheduleService
            .Setup(x => x.CreateDefaultSchedulesAsync(It.IsAny<int>()))
            .ReturnsAsync(FluentResults.Result.Ok());

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(It.IsAny<User>()))
            .Returns(new TokenPairDto
            {
                AccessToken = "token",
                RefreshToken = "refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 1, UserId = 1, Token = "refresh-token" });

        // Act
        await _authService.RegisterAsync(registerDto);

        // Assert
        _mockPasswordHasher.Verify(x => x.HashPassword(registerDto.Password), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)), Times.Once);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_Should_Return_Fail_When_User_Not_Found()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Email o contraseña incorrectos");
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Fail_When_Password_Is_Incorrect()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "wrongpassword"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = loginDto.Email,
            PasswordHash = "hashedpassword",
            BusinessId = 1,
            ProviderId = 1
        };

        _mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { existingUser });

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Email o contraseña incorrectos");
    }

    [Fact]
    public async Task LoginAsync_Should_Return_AuthResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "correctpassword"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = loginDto.Email,
            PasswordHash = "hashedpassword",
            BusinessId = 1,
            ProviderId = 1
        };

        _mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { existingUser });

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash))
            .Returns(true);

        var tokenPair = new TokenPairDto
        {
            AccessToken = "generated-access-token",
            RefreshToken = "generated-refresh-token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(existingUser))
            .Returns(tokenPair);

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken 
            { 
                Id = 1, 
                UserId = existingUser.Id, 
                Token = tokenPair.RefreshToken 
            });

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("generated-access-token");
        result.Value.RefreshToken.Should().Be("generated-refresh-token");
        result.Value.UserId.Should().Be(existingUser.Id);
        result.Value.Email.Should().Be(existingUser.Email);
        result.Value.BusinessId.Should().Be(existingUser.BusinessId);

        _mockPasswordHasher.Verify(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash), Times.Once);
        _mockJwtService.Verify(x => x.GenerateTokenPair(existingUser), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => 
            rt.UserId == existingUser.Id && 
            rt.Token == tokenPair.RefreshToken && 
            rt.ExpiresAt == tokenPair.RefreshTokenExpiresAt)), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_Should_Verify_Password_With_Correct_Parameters()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = loginDto.Email,
            PasswordHash = "stored_hashed_password",
            BusinessId = 1
        };

        _mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(new List<User> { existingUser });

        _mockPasswordHasher
            .Setup(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash))
            .Returns(true);

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(It.IsAny<User>()))
            .Returns(new TokenPairDto
            {
                AccessToken = "token",
                RefreshToken = "refresh-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            });

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 1, UserId = existingUser.Id, Token = "refresh-token" });

        // Act
        await _authService.LoginAsync(loginDto);

        // Assert
        _mockPasswordHasher.Verify(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash), Times.Once);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_Token_Format_Invalid()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-token-format"
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns((int?)null);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Refresh token inválido o expirado");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_Token_Not_Found_In_Database()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid.refresh.token"
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns(1); // UserId válido

        _mockRefreshTokenRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken>()); // No existe en la BD

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Refresh token no encontrado");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_Token_Is_Revoked()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "revoked.refresh.token"
        };

        var revokedToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "revoked.refresh.token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow.AddHours(-1) // Token revocado
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns(1);

        _mockRefreshTokenRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { revokedToken });

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Refresh token revocado");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_Fail_When_Token_Expired()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "expired.refresh.token"
        };

        var expiredToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "expired.refresh.token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expirado
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns(1);

        _mockRefreshTokenRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { expiredToken });

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Refresh token expirado");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_New_Tokens_When_Valid()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid.refresh.token"
        };

        var storedToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "valid.refresh.token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var existingUser = new User
        {
            Id = 1,
            Email = "user@example.com",
            BusinessId = 1,
            ProviderId = 1
        };

        var newTokenPair = new TokenPairDto
        {
            AccessToken = "new.access.token",
            RefreshToken = "new.refresh.token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns(1);

        _mockRefreshTokenRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { storedToken });

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingUser);

        _mockRefreshTokenRepository
            .Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(storedToken);

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(existingUser))
            .Returns(newTokenPair);

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken 
            { 
                Id = 2, 
                UserId = 1, 
                Token = newTokenPair.RefreshToken,
                ExpiresAt = newTokenPair.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("new.access.token");
        result.Value.RefreshToken.Should().Be("new.refresh.token");
        result.Value.UserId.Should().Be(existingUser.Id);
        result.Value.Email.Should().Be(existingUser.Email);
        result.Value.BusinessId.Should().Be(existingUser.BusinessId);

        _mockJwtService.Verify(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()), Times.Once);
        _mockUserRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(rt => rt.RevokedAt != null)), Times.Once);
        _mockJwtService.Verify(x => x.GenerateTokenPair(existingUser), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == existingUser.Id &&
            rt.Token == newTokenPair.RefreshToken &&
            rt.ExpiresAt == newTokenPair.RefreshTokenExpiresAt)), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Revoke_Old_Token_And_Create_New_One()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "old.refresh.token"
        };

        var storedToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "old.refresh.token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var existingUser = new User
        {
            Id = 1,
            Email = "user@example.com",
            BusinessId = 1,
            ProviderId = 1
        };

        var newTokenPair = new TokenPairDto
        {
            AccessToken = "new.access.token",
            RefreshToken = "new.refresh.token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockJwtService
            .Setup(x => x.ValidateRefreshToken(refreshTokenDto.RefreshToken))
            .Returns(1);

        _mockRefreshTokenRepository
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(new List<RefreshToken> { storedToken });

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingUser);

        _mockRefreshTokenRepository
            .Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(storedToken);

        _mockJwtService
            .Setup(x => x.GenerateTokenPair(existingUser))
            .Returns(newTokenPair);

        _mockRefreshTokenRepository
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken 
            { 
                Id = 2, 
                UserId = 1, 
                Token = newTokenPair.RefreshToken,
                ExpiresAt = newTokenPair.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        // Verificar que se revocó el token antiguo
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.Is<RefreshToken>(rt => 
            rt.Id == storedToken.Id && 
            rt.RevokedAt != null)), Times.Once);
        
        // Verificar que se creó un nuevo token
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == existingUser.Id &&
            rt.Token == newTokenPair.RefreshToken &&
            rt.ExpiresAt == newTokenPair.RefreshTokenExpiresAt)), Times.Once);
    }

    #endregion
}

