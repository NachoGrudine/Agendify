using Agendify.Controllers;
using Agendify.DTOs.Business;
using Agendify.Services.Business;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Agendify.Common.Errors;

namespace TestAgendify.Controllers;

public class BusinessControllerTests
{
    private readonly Mock<IBusinessService> _mockBusinessService;
    private readonly BusinessController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public BusinessControllerTests()
    {
        _mockBusinessService = new Mock<IBusinessService>();
        _sut = new BusinessController(_mockBusinessService.Object);
        
        // Setup HttpContext and User Claims for authentication
        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var claims = new List<Claim>
        {
            new Claim("BusinessId", BusinessId.ToString()),
            new Claim("UserId", UserId.ToString()),
            new Claim("Email", "test@example.com"),
            new Claim("ProviderId", "1")
        };
        
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
    public async Task Get_WhenBusinessExists_ReturnsOkWithBusiness()
    {
        // Arrange
        var expectedResponse = new BusinessResponseDto
        {
            Id = BusinessId,
            Name = "Test Business",
            Industry = "Technology"
        };
        
        _mockBusinessService
            .Setup(s => s.GetByIdAsync(BusinessId))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockBusinessService.Verify(
            s => s.GetByIdAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task Get_WhenBusinessNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockBusinessService
            .Setup(s => s.GetByIdAsync(BusinessId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Negocio no encontrado")));

        // Act
        var result = await _sut.Get();

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockBusinessService.Verify(
            s => s.GetByIdAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedBusiness()
    {
        // Arrange
        var updateDto = new UpdateBusinessDto
        {
            Name = "Updated Business Name",
            Industry = "Healthcare"
        };
        
        var expectedResponse = new BusinessResponseDto
        {
            Id = BusinessId,
            Name = updateDto.Name,
            Industry = updateDto.Industry
        };
        
        _mockBusinessService
            .Setup(s => s.UpdateAsync(BusinessId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Update(updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockBusinessService.Verify(
            s => s.UpdateAsync(BusinessId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateBusinessDto
        {
            Name = "", // Invalid: empty name
            Industry = "Technology"
        };
        
        _mockBusinessService
            .Setup(s => s.UpdateAsync(BusinessId, updateDto))
            .ReturnsAsync(Result.Fail("El nombre del negocio es requerido"));

        // Act
        var result = await _sut.Update(updateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockBusinessService.Verify(
            s => s.UpdateAsync(BusinessId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenBusinessNotFound_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateBusinessDto
        {
            Name = "Updated Business",
            Industry = "Technology"
        };
        
        _mockBusinessService
            .Setup(s => s.UpdateAsync(BusinessId, updateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Negocio no encontrado")));

        // Act
        var result = await _sut.Update(updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockBusinessService.Verify(
            s => s.UpdateAsync(BusinessId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Get_UsesBusinessIdFromClaims()
    {
        // Arrange
        var expectedResponse = new BusinessResponseDto
        {
            Id = BusinessId,
            Name = "Test Business",
            Industry = "Technology"
        };
        
        _mockBusinessService
            .Setup(s => s.GetByIdAsync(BusinessId))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        await _sut.Get();

        // Assert
        _mockBusinessService.Verify(
            s => s.GetByIdAsync(BusinessId),
            Times.Once,
            "Should use BusinessId from authenticated user claims");
    }

    [Fact]
    public async Task Update_UsesBusinessIdFromClaims()
    {
        // Arrange
        var updateDto = new UpdateBusinessDto
        {
            Name = "Updated Business",
            Industry = "Technology"
        };
        
        var expectedResponse = new BusinessResponseDto
        {
            Id = BusinessId,
            Name = updateDto.Name,
            Industry = updateDto.Industry
        };
        
        _mockBusinessService
            .Setup(s => s.UpdateAsync(BusinessId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        await _sut.Update(updateDto);

        // Assert
        _mockBusinessService.Verify(
            s => s.UpdateAsync(BusinessId, updateDto),
            Times.Once,
            "Should use BusinessId from authenticated user claims");
    }
}

