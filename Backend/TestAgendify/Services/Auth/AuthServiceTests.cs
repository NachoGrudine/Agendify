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
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IProviderScheduleService> _mockProviderScheduleService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockBusinessRepository = new Mock<IRepository<BusinessEntity>>();
        _mockProviderRepository = new Mock<IRepository<Provider>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtService = new Mock<IJwtService>();
        _mockProviderScheduleService = new Mock<IProviderScheduleService>();

        _authService = new AuthService(
            _mockUserRepository.Object,
            _mockBusinessRepository.Object,
            _mockProviderRepository.Object,
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

        _mockJwtService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("generated-jwt-token");

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be("generated-jwt-token");
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
        _mockJwtService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
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

        _mockJwtService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

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

        _mockJwtService
            .Setup(x => x.GenerateToken(existingUser))
            .Returns("generated-jwt-token");

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().Be("generated-jwt-token");
        result.Value.UserId.Should().Be(existingUser.Id);
        result.Value.Email.Should().Be(existingUser.Email);
        result.Value.BusinessId.Should().Be(existingUser.BusinessId);

        _mockPasswordHasher.Verify(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash), Times.Once);
        _mockJwtService.Verify(x => x.GenerateToken(existingUser), Times.Once);
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
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        // Act
        await _authService.LoginAsync(loginDto);

        // Assert
        _mockPasswordHasher.Verify(x => x.VerifyPassword(loginDto.Password, existingUser.PasswordHash), Times.Once);
    }

    #endregion
}

