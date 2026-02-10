using Agendify.Models.Entities;
using Agendify.Services.Auth.JWT;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TestAgendify.Services.Auth;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;
    private readonly string _testSecret = "ThisIsAVerySecureSecretKeyForJwtTokenGeneration12345";
    private readonly string _testIssuer = "TestIssuer";
    private readonly string _testAudience = "TestAudience";

    public JwtServiceTests()
    {
        // Usar ConfigurationBuilder en lugar de Mock para configuración compleja
        var configDict = new Dictionary<string, string>
        {
            { "Jwt:Secret", _testSecret },
            { "Jwt:Issuer", _testIssuer },
            { "Jwt:Audience", _testAudience },
            { "Jwt:AccessTokenExpirationMinutes", "15" },
            { "Jwt:RefreshTokenExpirationDays", "7" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateTokenPair_Should_Return_Both_Tokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        // Act
        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Assert
        tokenPair.Should().NotBeNull();
        tokenPair.AccessToken.Should().NotBeNullOrEmpty();
        tokenPair.RefreshToken.Should().NotBeNullOrEmpty();
        tokenPair.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        tokenPair.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_Should_Include_User_Claims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        // Act
        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenPair.AccessToken);

        var claims = jwtToken.Claims.ToList();
        
        claims.Should().Contain(c => c.Type == "UserId" && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == "Email" && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == "BusinessId" && c.Value == user.BusinessId.ToString());
        claims.Should().Contain(c => c.Type == "ProviderId" && c.Value == user.ProviderId.ToString());
        claims.Should().Contain(c => c.Type == "TokenType" && c.Value == "access");
    }

    [Fact]
    public void GenerateTokenPair_RefreshToken_Should_Include_Minimal_Claims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        // Act
        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenPair.RefreshToken);

        var claims = jwtToken.Claims.ToList();
        
        claims.Should().Contain(c => c.Type == "UserId" && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == "TokenType" && c.Value == "refresh");
        // Refresh token no debe tener email, businessId, providerId
        claims.Should().NotContain(c => c.Type == "Email");
        claims.Should().NotContain(c => c.Type == "BusinessId");
        claims.Should().NotContain(c => c.Type == "ProviderId");
    }

    [Fact]
    public void GenerateTokenPair_Should_Set_AccessToken_Expiration_To_15_Minutes()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenPair.AccessToken);

        var expectedExpiration = beforeGeneration.AddMinutes(15);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
        tokenPair.AccessTokenExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateTokenPair_Should_Set_RefreshToken_Expiration_To_7_Days()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenPair.RefreshToken);

        var expectedExpiration = beforeGeneration.AddDays(7);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
        tokenPair.RefreshTokenExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ValidateAccessToken_Should_Return_UserId_When_Token_Is_Valid()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Act
        var userId = _jwtService.ValidateAccessToken(tokenPair.AccessToken);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateRefreshToken_Should_Return_UserId_When_Token_Is_Valid()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Act
        var userId = _jwtService.ValidateRefreshToken(tokenPair.RefreshToken);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateAccessToken_Should_Return_Null_For_RefreshToken()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Act - Intentar validar refresh token como access token
        var userId = _jwtService.ValidateAccessToken(tokenPair.RefreshToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_Should_Return_Null_For_AccessToken()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var tokenPair = _jwtService.GenerateTokenPair(user);

        // Act - Intentar validar access token como refresh token
        var userId = _jwtService.ValidateRefreshToken(tokenPair.AccessToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_Should_Return_Null_When_Token_Is_Invalid()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var userId = _jwtService.ValidateAccessToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_Should_Return_Null_When_Token_Is_Invalid()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var userId = _jwtService.ValidateRefreshToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_Should_Return_Null_When_Token_Is_Expired()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_testSecret));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", "1"),
            new Claim("Email", "test@example.com"),
            new Claim("BusinessId", "10"),
            new Claim("ProviderId", "5"),
            new Claim("TokenType", "access")
        };

        var expiredToken = new JwtSecurityToken(
            issuer: _testIssuer,
            audience: _testAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-1), // Expired 1 minute ago
            signingCredentials: credentials
        );

        var expiredTokenString = handler.WriteToken(expiredToken);

        // Act
        var userId = _jwtService.ValidateAccessToken(expiredTokenString);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_Should_Return_Null_When_Token_Is_Empty()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var userId = _jwtService.ValidateAccessToken(emptyToken);

        // Assert
        userId.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("not.a.token")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
    public void ValidateAccessToken_Should_Return_Null_For_Malformed_Tokens(string malformedToken)
    {
        // Act
        var userId = _jwtService.ValidateAccessToken(malformedToken);

        // Assert
        userId.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("not.a.token")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
    public void ValidateRefreshToken_Should_Return_Null_For_Malformed_Tokens(string malformedToken)
    {
        // Act
        var userId = _jwtService.ValidateRefreshToken(malformedToken);

        // Assert
        userId.Should().BeNull();
    }
}

