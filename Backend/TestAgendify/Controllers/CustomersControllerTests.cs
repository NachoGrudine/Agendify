using Agendify.Controllers;
using Agendify.DTOs.Customer;
using Agendify.Services.Customers;
using Agendify.Common.Errors;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestAgendify.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly CustomersController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public CustomersControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _sut = new CustomersController(_mockCustomerService.Object);
        
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
    public async Task GetAll_ReturnsOkWithCustomers()
    {
        // Arrange
        var expectedCustomers = new List<CustomerResponseDto>
        {
            new CustomerResponseDto { Id = 1, BusinessId = BusinessId, Name = "John Doe", Email = "john@example.com" },
            new CustomerResponseDto { Id = 2, BusinessId = BusinessId, Name = "Jane Smith", Phone = "555-1234" }
        };
        
        _mockCustomerService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCustomers);
        
        _mockCustomerService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithNoCustomers_ReturnsOkWithEmptyList()
    {
        // Arrange
        var expectedCustomers = new List<CustomerResponseDto>();
        
        _mockCustomerService
            .Setup(s => s.GetByBusinessAsync(BusinessId))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _sut.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCustomers);
        
        _mockCustomerService.Verify(
            s => s.GetByBusinessAsync(BusinessId),
            Times.Once);
    }

    [Fact]
    public async Task Search_WithValidName_ReturnsOkWithMatchingCustomers()
    {
        // Arrange
        var searchName = "John";
        var expectedCustomers = new List<CustomerResponseDto>
        {
            new CustomerResponseDto { Id = 1, BusinessId = BusinessId, Name = "John Doe" },
            new CustomerResponseDto { Id = 3, BusinessId = BusinessId, Name = "Johnny Walker" }
        };
        
        _mockCustomerService
            .Setup(s => s.SearchByNameAsync(BusinessId, searchName))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _sut.Search(searchName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCustomers);
        
        _mockCustomerService.Verify(
            s => s.SearchByNameAsync(BusinessId, searchName),
            Times.Once);
    }

    [Fact]
    public async Task Search_WithNoMatches_ReturnsOkWithEmptyList()
    {
        // Arrange
        var searchName = "NonExistent";
        var expectedCustomers = new List<CustomerResponseDto>();
        
        _mockCustomerService
            .Setup(s => s.SearchByNameAsync(BusinessId, searchName))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _sut.Search(searchName);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCustomers);
        
        _mockCustomerService.Verify(
            s => s.SearchByNameAsync(BusinessId, searchName),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenCustomerExists_ReturnsOkWithCustomer()
    {
        // Arrange
        var customerId = 1;
        var expectedCustomer = new CustomerResponseDto
        {
            Id = customerId,
            BusinessId = BusinessId,
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "555-1234"
        };
        
        _mockCustomerService
            .Setup(s => s.GetByIdAsync(BusinessId, customerId))
            .ReturnsAsync(Result.Ok(expectedCustomer));

        // Act
        var result = await _sut.GetById(customerId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCustomer);
        
        _mockCustomerService.Verify(
            s => s.GetByIdAsync(BusinessId, customerId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        
        _mockCustomerService
            .Setup(s => s.GetByIdAsync(BusinessId, customerId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Cliente no encontrado")));

        // Act
        var result = await _sut.GetById(customerId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockCustomerService.Verify(
            s => s.GetByIdAsync(BusinessId, customerId),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "New Customer",
            Email = "newcustomer@example.com",
            Phone = "555-5678"
        };
        
        var expectedResponse = new CustomerResponseDto
        {
            Id = 1,
            BusinessId = BusinessId,
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone
        };
        
        _mockCustomerService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.ActionName.Should().Be(nameof(_sut.GetById));
        
        _mockCustomerService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "", // Invalid: empty name
            Email = "customer@example.com"
        };
        
        _mockCustomerService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Fail(new BadRequestError("El nombre es requerido")));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockCustomerService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedCustomer()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new UpdateCustomerDto
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "555-9999"
        };
        
        var expectedResponse = new CustomerResponseDto
        {
            Id = customerId,
            BusinessId = BusinessId,
            Name = updateDto.Name,
            Email = updateDto.Email,
            Phone = updateDto.Phone
        };
        
        _mockCustomerService
            .Setup(s => s.UpdateAsync(BusinessId, customerId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Update(customerId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockCustomerService.Verify(
            s => s.UpdateAsync(BusinessId, customerId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        var updateDto = new UpdateCustomerDto
        {
            Name = "Updated Name"
        };
        
        _mockCustomerService
            .Setup(s => s.UpdateAsync(BusinessId, customerId, updateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Cliente no encontrado")));

        // Act
        var result = await _sut.Update(customerId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockCustomerService.Verify(
            s => s.UpdateAsync(BusinessId, customerId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenCustomerExists_ReturnsNoContent()
    {
        // Arrange
        var customerId = 1;
        
        _mockCustomerService
            .Setup(s => s.DeleteAsync(BusinessId, customerId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _sut.Delete(customerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mockCustomerService.Verify(
            s => s.DeleteAsync(BusinessId, customerId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenCustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        
        _mockCustomerService
            .Setup(s => s.DeleteAsync(BusinessId, customerId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Cliente no encontrado")));

        // Act
        var result = await _sut.Delete(customerId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockCustomerService.Verify(
            s => s.DeleteAsync(BusinessId, customerId),
            Times.Once);
    }
}

