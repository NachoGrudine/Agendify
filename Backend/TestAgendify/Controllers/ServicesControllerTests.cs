using Agendify.Controllers;
using Agendify.DTOs.Service;
using Agendify.Services.ServicesServices;
using Agendify.Common.Errors;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestAgendify.Controllers;

public class ServicesControllerTests
{
    private readonly Mock<IServiceService> _mockServiceService;
    private readonly ServicesController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public ServicesControllerTests()
    {
        _mockServiceService = new Mock<IServiceService>();
        _sut = new ServicesController(_mockServiceService.Object);
        
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
    public async Task GetAll_ReturnsOkWithServices()
    {
        // Arrange
        var expectedServices = new List<ServiceResponseDto>
        {
            new ServiceResponseDto { Id = 1, BusinessId = BusinessId, Name = "Consultation", DefaultDuration = 30, Price = 100 },
            new ServiceResponseDto { Id = 2, BusinessId = BusinessId, Name = "Surgery", DefaultDuration = 120, Price = 500 }
        };
        
        _mockServiceService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedServices);
        
        _mockServiceService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithNoServices_ReturnsOkWithEmptyList()
    {
        // Arrange
        var expectedServices = new List<ServiceResponseDto>();
        
        _mockServiceService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedServices);
        
        _mockServiceService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task Search_WithValidName_ReturnsOkWithMatchingServices()
    {
        // Arrange
        var searchName = "Consul";
        var expectedServices = new List<ServiceResponseDto>
        {
            new ServiceResponseDto { Id = 1, BusinessId = BusinessId, Name = "Consultation", DefaultDuration = 30 },
            new ServiceResponseDto { Id = 3, BusinessId = BusinessId, Name = "Consultation Follow-up", DefaultDuration = 15 }
        };
        
        _mockServiceService
            .Setup(s => s.SearchByNameAsync(BusinessId, searchName))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.Search(searchName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedServices);
        
        _mockServiceService.Verify(
            s => s.SearchByNameAsync(BusinessId, searchName),
            Times.Once);
    }

    [Fact]
    public async Task Search_WithNoMatches_ReturnsOkWithEmptyList()
    {
        // Arrange
        var searchName = "NonExistent";
        var expectedServices = new List<ServiceResponseDto>();
        
        _mockServiceService
            .Setup(s => s.SearchByNameAsync(BusinessId, searchName))
            .ReturnsAsync(expectedServices);

        // Act
        var result = await _sut.Search(searchName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedServices);
        
        _mockServiceService.Verify(
            s => s.SearchByNameAsync(BusinessId, searchName),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceExists_ReturnsOkWithService()
    {
        // Arrange
        var serviceId = 1;
        var expectedService = new ServiceResponseDto
        {
            Id = serviceId,
            BusinessId = BusinessId,
            Name = "Consultation",
            DefaultDuration = 30,
            Price = 100
        };
        
        _mockServiceService
            .Setup(s => s.GetByIdAsync(BusinessId, serviceId))
            .ReturnsAsync(Result.Ok(expectedService));

        // Act
        var result = await _sut.GetById(serviceId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedService);
        
        _mockServiceService.Verify(
            s => s.GetByIdAsync(BusinessId, serviceId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceNotFound_ReturnsNotFound()
    {
        // Arrange
        var serviceId = 999;
        
        _mockServiceService
            .Setup(s => s.GetByIdAsync(BusinessId, serviceId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Servicio no encontrado")));

        // Act
        var result = await _sut.GetById(serviceId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockServiceService.Verify(
            s => s.GetByIdAsync(BusinessId, serviceId),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateServiceDto
        {
            Name = "New Service",
            DefaultDuration = 45,
            Price = 150
        };
        
        var expectedResponse = new ServiceResponseDto
        {
            Id = 1,
            BusinessId = BusinessId,
            Name = createDto.Name,
            DefaultDuration = createDto.DefaultDuration,
            Price = createDto.Price
        };
        
        _mockServiceService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.ActionName.Should().Be(nameof(_sut.GetById));
        
        _mockServiceService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateServiceDto
        {
            Name = "", // Invalid: empty name
            DefaultDuration = 30
        };
        
        _mockServiceService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Fail(new BadRequestError("El nombre es requerido")));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockServiceService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedService()
    {
        // Arrange
        var serviceId = 1;
        var updateDto = new UpdateServiceDto
        {
            Name = "Updated Service",
            DefaultDuration = 60,
            Price = 200
        };
        
        var expectedResponse = new ServiceResponseDto
        {
            Id = serviceId,
            BusinessId = BusinessId,
            Name = updateDto.Name,
            DefaultDuration = updateDto.DefaultDuration,
            Price = updateDto.Price
        };
        
        _mockServiceService
            .Setup(s => s.UpdateAsync(BusinessId, serviceId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockServiceService.Verify(
            s => s.UpdateAsync(BusinessId, serviceId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenServiceNotFound_ReturnsNotFound()
    {
        // Arrange
        var serviceId = 999;
        var updateDto = new UpdateServiceDto
        {
            Name = "Updated Service",
            DefaultDuration = 60
        };
        
        _mockServiceService
            .Setup(s => s.UpdateAsync(BusinessId, serviceId, updateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Servicio no encontrado")));

        // Act
        var result = await _sut.Update(serviceId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockServiceService.Verify(
            s => s.UpdateAsync(BusinessId, serviceId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenServiceExists_ReturnsNoContent()
    {
        // Arrange
        var serviceId = 1;
        
        _mockServiceService
            .Setup(s => s.DeleteAsync(BusinessId, serviceId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mockServiceService.Verify(
            s => s.DeleteAsync(BusinessId, serviceId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenServiceNotFound_ReturnsNotFound()
    {
        // Arrange
        var serviceId = 999;
        
        _mockServiceService
            .Setup(s => s.DeleteAsync(BusinessId, serviceId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Servicio no encontrado")));

        // Act
        var result = await _sut.Delete(serviceId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockServiceService.Verify(
            s => s.DeleteAsync(BusinessId, serviceId),
            Times.Once);
    }
}

