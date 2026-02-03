using Agendify.Common.Errors;
using Agendify.DTOs.Service;
using Agendify.Models.Entities;
using Agendify.Repositories;
using Agendify.Services.ServicesServices;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.ServicesServices;

public class ServiceServiceTests
{
    private readonly Mock<IRepository<Service>> _mockServiceRepository;
    private readonly ServiceService _sut;

    public ServiceServiceTests()
    {
        _mockServiceRepository = new Mock<IRepository<Service>>();
        _sut = new ServiceService(_mockServiceRepository.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateService()
    {
        // Arrange
        var businessId = 1;
        var dto = new CreateServiceDto
        {
            Name = "Haircut",
            DefaultDuration = 60,
            Price = 25.50m
        };

        var createdService = new Service
        {
            Id = 1,
            BusinessId = businessId,
            Name = dto.Name,
            DefaultDuration = dto.DefaultDuration,
            Price = dto.Price
        };

        _mockServiceRepository
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync(createdService);

        // Act
        var result = await _sut.CreateAsync(businessId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Haircut");
        result.Value.DefaultDuration.Should().Be(60);
        result.Value.Price.Should().Be(25.50m);

        _mockServiceRepository.Verify(x => x.AddAsync(It.Is<Service>(s =>
            s.BusinessId == businessId &&
            s.Name == dto.Name &&
            s.DefaultDuration == dto.DefaultDuration &&
            s.Price == dto.Price
        )), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenServiceExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;
        var dto = new UpdateServiceDto
        {
            Name = "Updated Service",
            DefaultDuration = 90,
            Price = 50.00m
        };

        var existingService = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Old Name",
            DefaultDuration = 60,
            Price = 25.00m
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(existingService);

        _mockServiceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Service>()))
            .ReturnsAsync(existingService);

        // Act
        var result = await _sut.UpdateAsync(businessId, serviceId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Service");
        result.Value.DefaultDuration.Should().Be(90);
        result.Value.Price.Should().Be(50.00m);

        _mockServiceRepository.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenServiceNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;
        var dto = new UpdateServiceDto { Name = "Test", DefaultDuration = 60 };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.UpdateAsync(businessId, serviceId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Be("Servicio no encontrado");

        _mockServiceRepository.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBusinessIdMismatch_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;
        var dto = new UpdateServiceDto { Name = "Test", DefaultDuration = 60 };

        var existingService = new Service
        {
            Id = serviceId,
            BusinessId = 999, // Different business
            Name = "Test"
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(existingService);

        // Act
        var result = await _sut.UpdateAsync(businessId, serviceId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenServiceExists_ShouldReturnService()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;

        var service = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Haircut",
            DefaultDuration = 60,
            Price = 25.00m
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.GetByIdAsync(businessId, serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(serviceId);
        result.Value.Name.Should().Be("Haircut");
    }

    [Fact]
    public async Task GetByIdAsync_WhenServiceNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.GetByIdAsync(businessId, serviceId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
    }

    #endregion

    #region GetByBusinessAsync Tests

    [Fact]
    public async Task GetByBusinessAsync_ShouldReturnAllServicesForBusiness()
    {
        // Arrange
        var businessId = 1;

        var services = new List<Service>
        {
            new() { Id = 1, BusinessId = businessId, Name = "Service 1", DefaultDuration = 60 },
            new() { Id = 2, BusinessId = businessId, Name = "Service 2", DefaultDuration = 90 },
            new() { Id = 3, BusinessId = businessId, Name = "Service 3", DefaultDuration = 30 }
        };

        _mockServiceRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(services);

        // Act
        var result = (await _sut.GetByBusinessAsync(businessId)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.BusinessId.Should().Be(businessId));
    }

    [Fact]
    public async Task GetByBusinessAsync_WhenNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;

        _mockServiceRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(new List<Service>());

        // Act
        var result = (await _sut.GetByBusinessAsync(businessId)).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region SearchByNameAsync Tests

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingServices()
    {
        // Arrange
        var businessId = 1;
        var searchTerm = "cut";

        var services = new List<Service>
        {
            new() { Id = 1, BusinessId = businessId, Name = "Haircut", DefaultDuration = 60 },
            new() { Id = 2, BusinessId = businessId, Name = "Beard Cut", DefaultDuration = 30 }
        };

        _mockServiceRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(services);

        // Act
        var result = (await _sut.SearchByNameAsync(businessId, searchTerm)).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchByNameAsync_WhenNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;
        var searchTerm = "nonexistent";

        _mockServiceRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>()))
            .ReturnsAsync(new List<Service>());

        // Act
        var result = (await _sut.SearchByNameAsync(businessId, searchTerm)).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenServiceExists_ShouldMarkAsDeleted()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;

        var service = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Test Service",
            IsDeleted = false
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(service);

        _mockServiceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Service>()))
            .ReturnsAsync(service);

        // Act
        var result = await _sut.DeleteAsync(businessId, serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.IsDeleted.Should().BeTrue();

        _mockServiceRepository.Verify(x => x.UpdateAsync(It.Is<Service>(s => s.IsDeleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenServiceNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 999;

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _sut.DeleteAsync(businessId, serviceId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _mockServiceRepository.Verify(x => x.UpdateAsync(It.IsAny<Service>()), Times.Never);
    }

    #endregion

    #region ResolveOrCreateAsync Tests

    [Fact]
    public async Task ResolveOrCreateAsync_WithValidServiceId_ShouldReturnExistingId()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;

        var existingService = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Existing Service",
            IsDeleted = false
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(existingService);

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, serviceId, null, 60);

        // Assert
        result.Should().Be(serviceId);

        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithServiceName_ShouldCreateNewService()
    {
        // Arrange
        var businessId = 1;
        var serviceName = "New Service";
        var defaultDuration = 90;

        var newService = new Service
        {
            Id = 99,
            BusinessId = businessId,
            Name = serviceName,
            DefaultDuration = defaultDuration
        };

        _mockServiceRepository
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync(newService);

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, null, serviceName, defaultDuration);

        // Assert
        result.Should().Be(99);

        _mockServiceRepository.Verify(x => x.AddAsync(It.Is<Service>(s =>
            s.BusinessId == businessId &&
            s.Name == serviceName &&
            s.DefaultDuration == defaultDuration &&
            s.Price == null
        )), Times.Once);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithInvalidIdButValidName_ShouldCreateNew()
    {
        // Arrange
        var businessId = 1;
        var invalidServiceId = 999;
        var serviceName = "New Service";
        var defaultDuration = 60;

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(invalidServiceId))
            .ReturnsAsync((Service?)null);

        var newService = new Service
        {
            Id = 100,
            BusinessId = businessId,
            Name = serviceName,
            DefaultDuration = defaultDuration
        };

        _mockServiceRepository
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync(newService);

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, invalidServiceId, serviceName, defaultDuration);

        // Assert
        result.Should().Be(100);

        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNoIdAndNoName_ShouldReturnNull()
    {
        // Arrange
        var businessId = 1;

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, null, null, 60);

        // Assert
        result.Should().BeNull();

        _mockServiceRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithDeletedService_ShouldCreateNew()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;
        var serviceName = "New Service";

        var deletedService = new Service
        {
            Id = serviceId,
            BusinessId = businessId,
            Name = "Deleted Service",
            IsDeleted = true
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(deletedService);

        var newService = new Service
        {
            Id = 101,
            BusinessId = businessId,
            Name = serviceName
        };

        _mockServiceRepository
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync(newService);

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, serviceId, serviceName, 60);

        // Assert
        result.Should().Be(101);

        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Once);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithServiceFromDifferentBusiness_ShouldCreateNew()
    {
        // Arrange
        var businessId = 1;
        var serviceId = 10;
        var serviceName = "New Service";

        var otherBusinessService = new Service
        {
            Id = serviceId,
            BusinessId = 999, // Different business
            Name = "Other Business Service"
        };

        _mockServiceRepository
            .Setup(x => x.GetByIdAsync(serviceId))
            .ReturnsAsync(otherBusinessService);

        var newService = new Service
        {
            Id = 102,
            BusinessId = businessId,
            Name = serviceName
        };

        _mockServiceRepository
            .Setup(x => x.AddAsync(It.IsAny<Service>()))
            .ReturnsAsync(newService);

        // Act
        var result = await _sut.ResolveOrCreateAsync(businessId, serviceId, serviceName, 60);

        // Assert
        result.Should().Be(102);

        _mockServiceRepository.Verify(x => x.AddAsync(It.IsAny<Service>()), Times.Once);
    }

    #endregion
}

