using Agendify.Models.Entities;
using Agendify.Services.Auth.JWT;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TestAgendify.Services.Auth;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtService _jwtService;
    private readonly string _testSecret = "ThisIsAVerySecureSecretKeyForJwtTokenGeneration12345";
    private readonly string _testIssuer = "TestIssuer";
    private readonly string _testAudience = "TestAudience";

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns(_testSecret);
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns(_testIssuer);
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns(_testAudience);

        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_Should_Return_Valid_Jwt_Token()
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
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be(_testIssuer);
        jwtToken.Audiences.Should().Contain(_testAudience);
    }

    [Fact]
    public void GenerateToken_Should_Include_User_Claims()
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
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();
        
        claims.Should().Contain(c => c.Type == "UserId" && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == "Email" && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == "BusinessId" && c.Value == user.BusinessId.ToString());
        claims.Should().Contain(c => c.Type == "ProviderId" && c.Value == user.ProviderId.ToString());
    }

    [Fact]
    public void GenerateToken_Should_Set_Expiration_To_7_Days()
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
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddDays(7);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ValidateToken_Should_Return_UserId_When_Token_Is_Valid()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@example.com",
            BusinessId = 10,
            ProviderId = 5
        };

        var token = _jwtService.GenerateToken(user);

        // Act
        var userId = _jwtService.ValidateToken(token);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_When_Token_Is_Invalid()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var userId = _jwtService.ValidateToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_When_Token_Is_Expired()
    {
        // Arrange
        // Create a mock configuration with the same settings
        var mockConfigForExpired = new Mock<IConfiguration>();
        mockConfigForExpired.Setup(x => x["Jwt:Secret"]).Returns(_testSecret);
        mockConfigForExpired.Setup(x => x["Jwt:Issuer"]).Returns(_testIssuer);
        mockConfigForExpired.Setup(x => x["Jwt:Audience"]).Returns(_testAudience);

        // Manually create an expired token
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
            new Claim("ProviderId", "5")
        };

        var expiredToken = new JwtSecurityToken(
            issuer: _testIssuer,
            audience: _testAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(-1), // Expired yesterday
            signingCredentials: credentials
        );

        var expiredTokenString = handler.WriteToken(expiredToken);

        // Act
        var userId = _jwtService.ValidateToken(expiredTokenString);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_When_Token_Is_Empty()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var userId = _jwtService.ValidateToken(emptyToken);

        // Assert
        userId.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("not.a.token")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
    public void ValidateToken_Should_Return_Null_For_Malformed_Tokens(string malformedToken)
    {
        // Act
        var userId = _jwtService.ValidateToken(malformedToken);

        // Assert
        userId.Should().BeNull();
    }
}

