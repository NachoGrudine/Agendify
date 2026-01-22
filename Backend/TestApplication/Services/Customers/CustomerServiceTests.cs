using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Customer;
using Agendify.API.Infrastructure.Repositories;
using Agendify.API.Services.Customers;
using FluentAssertions;
using Moq;

namespace TestApplication.Services.Customers;

public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _customerRepositoryMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerRepositoryMock = new Mock<IRepository<Customer>>();
        _customerService = new CustomerService(_customerRepositoryMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateCustomerDto
        {
            Name = "Juan Pérez",
            Phone = "1123456789",
            Email = "juan@email.com"
        };

        Customer? capturedCustomer = null;
        _customerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => capturedCustomer = c)
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _customerService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Phone.Should().Be(createDto.Phone);
        result.Email.Should().Be(createDto.Email);
        
        capturedCustomer.Should().NotBeNull();
        capturedCustomer!.BusinessId.Should().Be(businessId);
        capturedCustomer.Name.Should().Be(createDto.Name);
        capturedCustomer.IsDeleted.Should().BeFalse();
        
        _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithOptionalFieldsNull_ShouldCreateCustomer()
    {
        // Arrange
        var businessId = 1;
        var createDto = new CreateCustomerDto
        {
            Name = "María López",
            Phone = null,
            Email = null
        };

        _customerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _customerService.CreateAsync(businessId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Phone.Should().BeNull();
        result.Email.Should().BeNull();
    }

    #endregion
    [Fact]
    public async Task GetByIdAsync_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            BusinessId = businessId,
            Name = "Pedro García",
            Phone = "1134567890",
            Email = "pedro@email.com",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(businessId, customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be(customer.Name);
        result.Phone.Should().Be(customer.Phone);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCustomer_ShouldReturnNull()
    {
        // Arrange
        var businessId = 1;
        var customerId = 999;

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetByIdAsync(businessId, customerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentBusinessId_ShouldReturnNull()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            BusinessId = 2, // Diferente business
            Name = "Ana Torres",
            IsDeleted = false
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(businessId, customerId);

        // Assert
        result.Should().BeNull();
    }
    #region GetByBusinessAsync Tests

    [Fact]
    public async Task GetByBusinessAsync_ShouldReturnAllCustomersFromBusiness()
    {
        // Arrange
        var businessId = 1;
        var customers = new List<Customer>
        {
            new() { Id = 1, BusinessId = businessId, Name = "Cliente 1", IsDeleted = false },
            new() { Id = 2, BusinessId = businessId, Name = "Cliente 2", IsDeleted = false },
            new() { Id = 3, BusinessId = businessId, Name = "Cliente 3", IsDeleted = false }
        };

        _customerRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(c => c.Should().NotBeNull());
    }

    [Fact]
    public async Task GetByBusinessAsync_WithNoCustomers_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;

        _customerRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
            .ReturnsAsync(new List<Customer>());

        // Act
        var result = await _customerService.GetByBusinessAsync(businessId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var existingCustomer = new Customer
        {
            Id = customerId,
            BusinessId = businessId,
            Name = "Nombre Viejo",
            Phone = "1111111111",
            Email = "viejo@email.com",
            IsDeleted = false
        };

        var updateDto = new UpdateCustomerDto
        {
            Name = "Nombre Nuevo",
            Phone = "2222222222",
            Email = "nuevo@email.com"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(existingCustomer);

        Customer? updatedCustomer = null;
        _customerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => updatedCustomer = c)
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _customerService.UpdateAsync(businessId, customerId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updateDto.Name);
        result.Phone.Should().Be(updateDto.Phone);
        result.Email.Should().Be(updateDto.Email);

        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Name.Should().Be(updateDto.Name);

        _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var businessId = 1;
        var customerId = 999;
        var updateDto = new UpdateCustomerDto { Name = "Test" };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () => await _customerService.UpdateAsync(businessId, customerId, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Cliente no encontrado");
    }

    [Fact]
    public async Task UpdateAsync_WithDifferentBusinessId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            BusinessId = 2, // Diferente business
            Name = "Test"
        };
        var updateDto = new UpdateCustomerDto { Name = "Nuevo" };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        Func<Task> act = async () => await _customerService.UpdateAsync(businessId, customerId, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingCustomer_ShouldMarkAsDeleted()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            BusinessId = businessId,
            Name = "Test",
            IsDeleted = false
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        Customer? deletedCustomer = null;
        _customerRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => deletedCustomer = c)
            .ReturnsAsync((Customer c) => c);

        // Act
        await _customerService.DeleteAsync(businessId, customerId);

        // Assert
        deletedCustomer.Should().NotBeNull();
        deletedCustomer!.IsDeleted.Should().BeTrue();

        _customerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingCustomer_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var businessId = 1;
        var customerId = 999;

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        Func<Task> act = async () => await _customerService.DeleteAsync(businessId, customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Cliente no encontrado");
    }

    [Fact]
    public async Task DeleteAsync_WithDifferentBusinessId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var businessId = 1;
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            BusinessId = 2,
            Name = "Test"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        Func<Task> act = async () => await _customerService.DeleteAsync(businessId, customerId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion
}

