using Agendify.Controllers;
using Agendify.DTOs.Provider;
using Agendify.Services.Providers;
using Agendify.Common.Errors;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestAgendify.Controllers;

public class ProvidersControllerTests
{
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly ProvidersController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public ProvidersControllerTests()
    {
        _mockProviderService = new Mock<IProviderService>();
        _sut = new ProvidersController(_mockProviderService.Object);
        
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
    public async Task GetAll_ReturnsOkWithProviders()
    {
        // Arrange
        var expectedProviders = new List<ProviderResponseDto>
        {
            new ProviderResponseDto { Id = 1, BusinessId = BusinessId, Name = "Dr. Smith", Specialty = "Cardiology", IsActive = true },
            new ProviderResponseDto { Id = 2, BusinessId = BusinessId, Name = "Dr. Jones", Specialty = "Pediatrics", IsActive = true }
        };
        
        _mockProviderService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedProviders);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProviders);
        
        _mockProviderService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithNoProviders_ReturnsOkWithEmptyList()
    {
        // Arrange
        var expectedProviders = new List<ProviderResponseDto>();
        
        _mockProviderService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedProviders);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProviders);
        
        _mockProviderService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenProviderExists_ReturnsOkWithProvider()
    {
        // Arrange
        var providerId = 1;
        var expectedProvider = new ProviderResponseDto
        {
            Id = providerId,
            BusinessId = BusinessId,
            Name = "Dr. Smith",
            Specialty = "Cardiology",
            IsActive = true
        };
        
        _mockProviderService
            .Setup(s => s.GetByIdAsync(BusinessId, providerId))
            .ReturnsAsync(Result.Ok(expectedProvider));

        // Act
        var result = await _sut.GetById(providerId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProvider);
        
        _mockProviderService.Verify(
            s => s.GetByIdAsync(BusinessId, providerId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenProviderNotFound_ReturnsNotFound()
    {
        // Arrange
        var providerId = 999;
        
        _mockProviderService
            .Setup(s => s.GetByIdAsync(BusinessId, providerId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Proveedor no encontrado")));

        // Act
        var result = await _sut.GetById(providerId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockProviderService.Verify(
            s => s.GetByIdAsync(BusinessId, providerId),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateProviderDto
        {
            Name = "Dr. New Provider",
            Specialty = "Neurology"
        };
        
        var expectedResponse = new ProviderResponseDto
        {
            Id = 1,
            BusinessId = BusinessId,
            Name = createDto.Name,
            Specialty = createDto.Specialty,
            IsActive = true
        };
        
        _mockProviderService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.ActionName.Should().Be(nameof(_sut.GetById));
        
        _mockProviderService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateProviderDto
        {
            Name = "", // Invalid: empty name
            Specialty = "Cardiology"
        };
        
        _mockProviderService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Fail(new BadRequestError("El nombre es requerido")));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockProviderService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedProvider()
    {
        // Arrange
        var providerId = 1;
        var updateDto = new UpdateProviderDto
        {
            Name = "Dr. Updated Name",
            Specialty = "Updated Specialty",
            IsActive = false
        };
        
        var expectedResponse = new ProviderResponseDto
        {
            Id = providerId,
            BusinessId = BusinessId,
            Name = updateDto.Name,
            Specialty = updateDto.Specialty,
            IsActive = updateDto.IsActive
        };
        
        _mockProviderService
            .Setup(s => s.UpdateAsync(BusinessId, providerId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Update(providerId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockProviderService.Verify(
            s => s.UpdateAsync(BusinessId, providerId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenProviderNotFound_ReturnsNotFound()
    {
        // Arrange
        var providerId = 999;
        var updateDto = new UpdateProviderDto
        {
            Name = "Updated Name",
            Specialty = "Updated Specialty"
        };
        
        _mockProviderService
            .Setup(s => s.UpdateAsync(BusinessId, providerId, updateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Proveedor no encontrado")));

        // Act
        var result = await _sut.Update(providerId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockProviderService.Verify(
            s => s.UpdateAsync(BusinessId, providerId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenProviderExists_ReturnsNoContent()
    {
        // Arrange
        var providerId = 1;
        
        _mockProviderService
            .Setup(s => s.DeleteAsync(BusinessId, providerId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _sut.Delete(providerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mockProviderService.Verify(
            s => s.DeleteAsync(BusinessId, providerId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenProviderNotFound_ReturnsNotFound()
    {
        // Arrange
        var providerId = 999;
        
        _mockProviderService
            .Setup(s => s.DeleteAsync(BusinessId, providerId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Proveedor no encontrado")));

        // Act
        var result = await _sut.Delete(providerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockProviderService.Verify(
            s => s.DeleteAsync(BusinessId, providerId),
            Times.Once);
    }
}

