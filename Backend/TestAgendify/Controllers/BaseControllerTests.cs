using Agendify.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TestAgendify.Controllers;

public class BaseControllerTests
{
    private class TestController : BaseController
    {
        public int TestGetBusinessId() => GetBusinessId();
        public int TestGetUserId() => GetUserId();
        public string? TestGetUserEmail() => GetUserEmail();
        public int TestGetProviderId() => GetProviderId();
    }

    private readonly TestController _sut;

    public BaseControllerTests()
    {
        _sut = new TestController();
    }

    private void SetupControllerContext(List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public void GetBusinessId_WithValidClaim_ReturnsBusinessId()
    {
        // Arrange
        const int expectedBusinessId = 123;
        var claims = new List<Claim>
        {
            new Claim("BusinessId", expectedBusinessId.ToString())
        };
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetBusinessId();

        // Assert
        result.Should().Be(expectedBusinessId);
    }

    [Fact]
    public void GetBusinessId_WithoutClaim_ReturnsZero()
    {
        // Arrange
        var claims = new List<Claim>();
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetBusinessId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetUserId_WithValidClaim_ReturnsUserId()
    {
        // Arrange
        const int expectedUserId = 456;
        var claims = new List<Claim>
        {
            new Claim("UserId", expectedUserId.ToString())
        };
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetUserId_WithoutClaim_ReturnsZero()
    {
        // Arrange
        var claims = new List<Claim>();
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetUserId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetUserEmail_WithValidClaim_ReturnsEmail()
    {
        // Arrange
        const string expectedEmail = "test@example.com";
        var claims = new List<Claim>
        {
            new Claim("Email", expectedEmail)
        };
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetUserEmail();

        // Assert
        result.Should().Be(expectedEmail);
    }

    [Fact]
    public void GetUserEmail_WithoutClaim_ReturnsNull()
    {
        // Arrange
        var claims = new List<Claim>();
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetUserEmail();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetProviderId_WithValidClaim_ReturnsProviderId()
    {
        // Arrange
        const int expectedProviderId = 789;
        var claims = new List<Claim>
        {
            new Claim("ProviderId", expectedProviderId.ToString())
        };
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetProviderId();

        // Assert
        result.Should().Be(expectedProviderId);
    }

    [Fact]
    public void GetProviderId_WithoutClaim_ReturnsZero()
    {
        // Arrange
        var claims = new List<Claim>();
        SetupControllerContext(claims);

        // Act
        var result = _sut.TestGetProviderId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetAllClaims_WithMultipleClaims_ReturnsCorrectValues()
    {
        // Arrange
        const int expectedBusinessId = 100;
        const int expectedUserId = 200;
        const string expectedEmail = "multi@example.com";
        const int expectedProviderId = 300;
        
        var claims = new List<Claim>
        {
            new Claim("BusinessId", expectedBusinessId.ToString()),
            new Claim("UserId", expectedUserId.ToString()),
            new Claim("Email", expectedEmail),
            new Claim("ProviderId", expectedProviderId.ToString())
        };
        SetupControllerContext(claims);

        // Act & Assert
        _sut.TestGetBusinessId().Should().Be(expectedBusinessId);
        _sut.TestGetUserId().Should().Be(expectedUserId);
        _sut.TestGetUserEmail().Should().Be(expectedEmail);
        _sut.TestGetProviderId().Should().Be(expectedProviderId);
    }
}

