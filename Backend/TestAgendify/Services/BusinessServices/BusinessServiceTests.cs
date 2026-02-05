using Agendify.Common.Errors;
using Agendify.DTOs.Business;
using Agendify.Repositories;
using Agendify.Services.Business;
using BusinessEntity = Agendify.Models.Entities.Business;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.BusinessServices;

public class BusinessServiceTests
{
    private readonly Mock<IRepository<BusinessEntity>> _mockBusinessRepository;
    private readonly BusinessService _businessService;

    public BusinessServiceTests()
    {
        _mockBusinessRepository = new Mock<IRepository<BusinessEntity>>();
        _businessService = new BusinessService(_mockBusinessRepository.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFoundError_When_Business_Does_Not_Exist()
    {
        // Arrange
        var businessId = 1;
        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync((BusinessEntity?)null);

        // Act
        var result = await _businessService.GetByIdAsync(businessId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Be("Negocio no encontrado");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_BusinessResponseDto_When_Business_Exists()
    {
        // Arrange
        var businessId = 1;
        var business = new BusinessEntity
        {
            Id = businessId,
            Name = "My Business",
            Industry = "Technology"
        };

        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(business);

        // Act
        var result = await _businessService.GetByIdAsync(businessId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(business.Id);
        result.Value.Name.Should().Be(business.Name);
        result.Value.Industry.Should().Be(business.Industry);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Call_Repository_With_Correct_Id()
    {
        // Arrange
        var businessId = 42;
        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(new BusinessEntity { Id = businessId, Name = "Test", Industry = "Tech" });

        // Act
        await _businessService.GetByIdAsync(businessId);

        // Assert
        _mockBusinessRepository.Verify(x => x.GetByIdAsync(businessId), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_Should_Return_NotFoundError_When_Business_Does_Not_Exist()
    {
        // Arrange
        var businessId = 1;
        var updateDto = new UpdateBusinessDto
        {
            Name = "Updated Business",
            Industry = "Updated Industry"
        };

        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync((BusinessEntity?)null);

        // Act
        var result = await _businessService.UpdateAsync(businessId, updateDto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<NotFoundError>()
            .Which.Message.Should().Be("Negocio no encontrado");
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Business_Properties()
    {
        // Arrange
        var businessId = 1;
        var existingBusiness = new BusinessEntity
        {
            Id = businessId,
            Name = "Old Name",
            Industry = "Old Industry"
        };

        var updateDto = new UpdateBusinessDto
        {
            Name = "New Name",
            Industry = "New Industry"
        };

        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(existingBusiness);

        _mockBusinessRepository
            .Setup(x => x.UpdateAsync(It.IsAny<BusinessEntity>()))
            .ReturnsAsync((BusinessEntity b) => b);

        // Act
        var result = await _businessService.UpdateAsync(businessId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockBusinessRepository.Verify(x => x.UpdateAsync(It.Is<BusinessEntity>(b =>
            b.Id == businessId &&
            b.Name == updateDto.Name &&
            b.Industry == updateDto.Industry
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Updated_BusinessResponseDto()
    {
        // Arrange
        var businessId = 1;
        var existingBusiness = new BusinessEntity
        {
            Id = businessId,
            Name = "Old Name",
            Industry = "Old Industry"
        };

        var updateDto = new UpdateBusinessDto
        {
            Name = "New Name",
            Industry = "New Industry"
        };

        var updatedBusiness = new BusinessEntity
        {
            Id = businessId,
            Name = updateDto.Name,
            Industry = updateDto.Industry
        };

        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(existingBusiness);

        _mockBusinessRepository
            .Setup(x => x.UpdateAsync(It.IsAny<BusinessEntity>()))
            .ReturnsAsync(updatedBusiness);

        // Act
        var result = await _businessService.UpdateAsync(businessId, updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(updatedBusiness.Id);
        result.Value.Name.Should().Be(updatedBusiness.Name);
        result.Value.Industry.Should().Be(updatedBusiness.Industry);
    }

    [Fact]
    public async Task UpdateAsync_Should_Call_Repository_GetByIdAsync_Before_Update()
    {
        // Arrange
        var businessId = 1;
        var updateDto = new UpdateBusinessDto
        {
            Name = "New Name",
            Industry = "New Industry"
        };

        var business = new BusinessEntity { Id = businessId, Name = "Old", Industry = "Old" };

        _mockBusinessRepository
            .Setup(x => x.GetByIdAsync(businessId))
            .ReturnsAsync(business);

        _mockBusinessRepository
            .Setup(x => x.UpdateAsync(It.IsAny<BusinessEntity>()))
            .ReturnsAsync(business);

        // Act
        await _businessService.UpdateAsync(businessId, updateDto);

        // Assert
        _mockBusinessRepository.Verify(x => x.GetByIdAsync(businessId), Times.Once);
        _mockBusinessRepository.Verify(x => x.UpdateAsync(It.IsAny<BusinessEntity>()), Times.Once);
    }

    #endregion
}

