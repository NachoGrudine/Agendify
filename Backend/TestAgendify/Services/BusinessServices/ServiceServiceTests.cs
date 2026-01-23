using Agendify.Models.Entities;
using Agendify.DTOs.Service;
using Agendify.Repositories;
using Agendify.Services.ServicesServices;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.BusinessServices;

public class ServiceServiceTests
{
    private readonly Mock<IRepository<Service>> _serviceRepositoryMock;
    private readonly ServiceService _serviceService;

    public ServiceServiceTests()
    {
        _serviceRepositoryMock = new Mock<IRepository<Service>>();
        _serviceService = new ServiceService(_serviceRepositoryMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateService()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateServiceDto
        {
            Name = "Corte de Cabello",
            DefaultDuration = 30,
            Price = 150.50m
        };

        Service? capturedService = null;
        _serviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .Callback<Service>(s => capturedService = s)
            .ReturnsAsync((Service s) => s);

        // Act
        var result = await _serviceService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Value.Name.Should().Be(createDto.Name);
        result.Value.DefaultDuration.Should().Be(createDto.DefaultDuration);
        result.Value.Price.Should().Be(createDto.Price);

        capturedService.Should().NotBeNull();
        capturedService!.BusinessId.Should().Be(businessId);
        capturedService.IsDeleted.Should().BeFalse();

        _serviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithZeroPrice_ShouldCreateService()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateServiceDto
        {
            Name = "Consulta Gratis",
            DefaultDuration = 15,
            Price = 0
        };

        _serviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync((Service s) => s);

        // Act
        var result = await _serviceService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Value.Price.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingService_ShouldReturnService()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 1;
        var service = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Peinado",
            DefaultDuration = 45,
            Price = 200.00m,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(service);

        // Act
        var result = await _serviceService.GetByIdAsync(businessId, serviceId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(serviceId);
        result.Value.Name.Should().Be(service.Name);
        result.Value.DefaultDuration.Should().Be(service.DefaultDuration);
        result.Value.Price.Should().Be(service.Price);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _serviceService.GetByIdAsync(businessId, serviceId);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentBusinessId_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 1;
        var service = new Service
        {
            Id = serviceId,
            BusinessId = 2,
            Name = "Test",
            DefaultDuration = 30,
            Price = 100,
            IsDeleted = false
        };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(service);

        // Act
        var result = await _serviceService.GetByIdAsync(businessId, serviceId);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    #endregion

    #region GetByBusinessAsync Tests

    [Fact]
    public async Task GetByBusinessAsync_ShouldReturnAllServicesFromBusiness()
    {
        // Arrange
        var businessId = 1;
        var services = new List<Service>
        {
            new() { Id = 1, BusinessId = businessId, Name = "Servicio 1", DefaultDuration = 30, Price = 100, IsDeleted = false },
            new() { Id = 2, BusinessId = businessId, Name = "Servicio 2", DefaultDuration = 60, Price = 200, IsDeleted = false },
            new() { Id = 3, BusinessId = businessId, Name = "Servicio 3", DefaultDuration = 90, Price = 300, IsDeleted = false }
        };

        _serviceRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(services);

        // Act
        var result = await _serviceService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.Should().NotBeNull());
    }

    [Fact]
    public async Task GetByBusinessAsync_WithNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;

        _serviceRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(new List<Service>());

        // Act
        var result = await _serviceService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateService()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 1;
        var existingService = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Nombre Viejo",
            DefaultDuration = 30,
            Price = 100,
            IsDeleted = false
        };

        var updateDto = new UpdateServiceDto
        {
            Name = "Nombre Nuevo",
            DefaultDuration = 45,
            Price = 150
        };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(existingService);

        Service? updatedService = null;
        _serviceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Service>()))
            .Callback<Service>(s => updatedService = s)
            .ReturnsAsync((Service s) => s);

        // Act
        var result = await _serviceService.UpdateAsync(businessId, serviceId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Value.Name.Should().Be(updateDto.Name);
        result.Value.DefaultDuration.Should().Be(updateDto.DefaultDuration);
        result.Value.Price.Should().Be(updateDto.Price);

        updatedService.Should().NotBeNull();

        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;
        var updateDto = new UpdateServiceDto { Name = "Test", DefaultDuration = 30, Price = 100 };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _serviceService.UpdateAsync(businessId, serviceId, updateDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Servicio no encontrado");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingService_ShouldMarkAsDeleted()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 1;
        var service = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Test",
            DefaultDuration = 30,
            Price = 100,
            IsDeleted = false
        };

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(service);

        Service? deletedService = null;
        _serviceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Service>()))
            .Callback<Service>(s => deletedService = s)
            .ReturnsAsync((Service s) => s);

        // Act
        var result = await _serviceService.DeleteAsync(businessId, serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedService.Should().NotBeNull();
        deletedService!.IsDeleted.Should().BeTrue();

        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _serviceService.DeleteAsync(businessId, serviceId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Servicio no encontrado");
    }

    #endregion
}

