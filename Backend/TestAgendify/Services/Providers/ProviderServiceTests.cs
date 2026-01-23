using Agendify.Models.Entities;
using Agendify.DTOs.Provider;
using Agendify.Repositories;
using Agendify.Services.Providers;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.Providers;

public class ProviderServiceTests
{
    private readonly Mock<IRepository<Provider>> _providerRepositoryMock;
    private readonly ProviderService _providerService;

    public ProviderServiceTests()
    {
        _providerRepositoryMock = new Mock<IRepository<Provider>>();
        _providerService = new ProviderService(_providerRepositoryMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateProvider()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateProviderDto
        {
            Name = "Dr. Juan P�rez",
            Specialty = "Cardiolog�a"
        };

        Provider? capturedProvider = null;
        _providerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Provider>()))
            .Callback<Provider>(p => capturedProvider = p)
            .ReturnsAsync((Provider p) => p);

        // Act
        var result = await _providerService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Value.Name.Should().Be(createDto.Name);
        result.Value.Specialty.Should().Be(createDto.Specialty);
        result.Value.IsActive.Should().BeTrue(); // Default should be true

        capturedProvider.Should().NotBeNull();
        capturedProvider!.BusinessId.Should().Be(businessId);
        capturedProvider.IsDeleted.Should().BeFalse();
        capturedProvider.IsActive.Should().BeTrue();

        _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithOptionalSpecialty_ShouldCreateProvider()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateProviderDto
        {
            Name = "Mar�a L�pez",
            Specialty = null
        };

        _providerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Provider>()))
            .ReturnsAsync((Provider p) => p);

        // Act
        var result = await _providerService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Value.Specialty.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        var businessId = 1;
        var providerId = 1;
        var provider = new Provider
        {
            Id = providerId,
            BusinessId = businessId,
            Name = "Dr. Pedro Garc�a",
            Specialty = "Dermatolog�a",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(provider);

        // Act
        var result = await _providerService.GetByIdAsync(businessId, providerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(providerId);
        result.Value.Name.Should().Be(provider.Name);
        result.Value.Specialty.Should().Be(provider.Specialty);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingProvider_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 999;

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _providerService.GetByIdAsync(businessId, providerId);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentBusinessId_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 1;
        var provider = new Provider
        {
            Id = providerId,
            BusinessId = 2,
            Name = "Test",
            IsDeleted = false
        };

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(provider);

        // Act
        var result = await _providerService.GetByIdAsync(businessId, providerId);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    #endregion

    #region GetByBusinessAsync Tests

    [Fact]
    public async Task GetByBusinessAsync_ShouldReturnAllProvidersFromBusiness()
    {
        // Arrange
        var businessId = 1;
        var providers = new List<Provider>
        {
            new() { Id = 1, BusinessId = businessId, Name = "Provider 1", IsActive = true, IsDeleted = false },
            new() { Id = 2, BusinessId = businessId, Name = "Provider 2", IsActive = true, IsDeleted = false },
            new() { Id = 3, BusinessId = businessId, Name = "Provider 3", IsActive = false, IsDeleted = false }
        };

        _providerRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _providerService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task GetByBusinessAsync_WithNoProviders_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;

        _providerRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Provider, bool>>>()))
            .ReturnsAsync(new List<Provider>());

        // Act
        var result = await _providerService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateProvider()
    {
        // Arrange
        var businessId = 1;
        var providerId = 1;
        var existingProvider = new Provider
        {
            Id = providerId,
            BusinessId = businessId,
            Name = "Nombre Viejo",
            Specialty = "Especialidad Vieja",
            IsActive = true,
            IsDeleted = false
        };

        var updateDto = new UpdateProviderDto
        {
            Name = "Nombre Nuevo",
            Specialty = "Especialidad Nueva",
            IsActive = false
        };

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(existingProvider);

        Provider? updatedProvider = null;
        _providerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>()))
            .Callback<Provider>(p => updatedProvider = p)
            .ReturnsAsync((Provider p) => p);

        // Act
        var result = await _providerService.UpdateAsync(businessId, providerId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(updateDto.Name);
        result.Value.Specialty.Should().Be(updateDto.Specialty);
        result.Value.IsActive.Should().Be(updateDto.IsActive);

        updatedProvider.Should().NotBeNull();

        _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingProvider_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 999;
        var updateDto = new UpdateProviderDto { Name = "Test" };

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _providerService.UpdateAsync(businessId, providerId, updateDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Proveedor no encontrado");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingProvider_ShouldMarkAsDeleted()
    {
        // Arrange
        var businessId = 1;
        var providerId = 1;
        var provider = new Provider
        {
            Id = providerId,
            BusinessId = businessId,
            Name = "Test",
            IsActive = true,
            IsDeleted = false
        };

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(provider);

        Provider? deletedProvider = null;
        _providerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>()))
            .Callback<Provider>(p => deletedProvider = p)
            .ReturnsAsync((Provider p) => p);

        // Act
        var result = await _providerService.DeleteAsync(businessId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        deletedProvider.Should().NotBeNull();
        deletedProvider!.IsDeleted.Should().BeTrue();

        _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingProvider_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 999;

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _providerService.DeleteAsync(businessId, providerId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Proveedor no encontrado");
    }

    #endregion
}

