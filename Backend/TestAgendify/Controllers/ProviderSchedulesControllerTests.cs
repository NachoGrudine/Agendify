using Agendify.Controllers;
using Agendify.DTOs.ProviderSchedule;
using Agendify.Services.ProviderSchedules;
using Agendify.Common.Errors;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace TestAgendify.Controllers;

public class ProviderSchedulesControllerTests
{
    private readonly Mock<IProviderScheduleService> _mockScheduleService;
    private readonly ProviderSchedulesController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;
    private const int ProviderId = 5;

    public ProviderSchedulesControllerTests()
    {
        _mockScheduleService = new Mock<IProviderScheduleService>();
        _sut = new ProviderSchedulesController(_mockScheduleService.Object);
        
        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var claims = new List<Claim>
        {
            new Claim("BusinessId", BusinessId.ToString()),
            new Claim("UserId", UserId.ToString()),
            new Claim("Email", "test@example.com"),
            new Claim("ProviderId", ProviderId.ToString())
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
    public async Task GetMySchedules_ReturnsOkWithSchedules()
    {
        // Arrange
        var expectedSchedules = new List<ProviderScheduleResponseDto>
        {
            new ProviderScheduleResponseDto 
            { 
                Id = 1, 
                ProviderId = ProviderId, 
                DayOfWeek = DayOfWeek.Monday, 
                StartTime = new TimeSpan(9, 0, 0), 
                EndTime = new TimeSpan(17, 0, 0) 
            },
            new ProviderScheduleResponseDto 
            { 
                Id = 2, 
                ProviderId = ProviderId, 
                DayOfWeek = DayOfWeek.Tuesday, 
                StartTime = new TimeSpan(9, 0, 0), 
                EndTime = new TimeSpan(17, 0, 0) 
            }
        };
        
        _mockScheduleService
            .Setup(s => s.GetByProviderAsync(BusinessId, ProviderId))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        var result = await _sut.GetMySchedules();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSchedules);
        
        _mockScheduleService.Verify(
            s => s.GetByProviderAsync(BusinessId, ProviderId),
            Times.Once);
    }

    [Fact]
    public async Task GetMySchedules_WhenNoSchedules_ReturnsOkWithEmptyList()
    {
        // Arrange
        var expectedSchedules = new List<ProviderScheduleResponseDto>();
        
        _mockScheduleService
            .Setup(s => s.GetByProviderAsync(BusinessId, ProviderId))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        var result = await _sut.GetMySchedules();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSchedules);
        
        _mockScheduleService.Verify(
            s => s.GetByProviderAsync(BusinessId, ProviderId),
            Times.Once);
    }

    [Fact]
    public async Task GetByProvider_WithValidProviderId_ReturnsOkWithSchedules()
    {
        // Arrange
        var targetProviderId = 10;
        var expectedSchedules = new List<ProviderScheduleResponseDto>
        {
            new ProviderScheduleResponseDto 
            { 
                Id = 1, 
                ProviderId = targetProviderId, 
                DayOfWeek = DayOfWeek.Wednesday, 
                StartTime = new TimeSpan(8, 0, 0), 
                EndTime = new TimeSpan(16, 0, 0) 
            }
        };
        
        _mockScheduleService
            .Setup(s => s.GetByProviderAsync(BusinessId, targetProviderId))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        var result = await _sut.GetByProvider(targetProviderId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSchedules);
        
        _mockScheduleService.Verify(
            s => s.GetByProviderAsync(BusinessId, targetProviderId),
            Times.Once);
    }

    [Fact]
    public async Task GetByProvider_WhenProviderNotFound_ReturnsNotFound()
    {
        // Arrange
        var targetProviderId = 999;
        
        _mockScheduleService
            .Setup(s => s.GetByProviderAsync(BusinessId, targetProviderId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Proveedor no encontrado")));

        // Act
        var result = await _sut.GetByProvider(targetProviderId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockScheduleService.Verify(
            s => s.GetByProviderAsync(BusinessId, targetProviderId),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpdateMySchedules_WithValidData_ReturnsOkWithUpdatedSchedules()
    {
        // Arrange
        var bulkUpdateDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto 
                { 
                    DayOfWeek = DayOfWeek.Monday, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0) 
                },
                new ProviderScheduleItemDto 
                { 
                    DayOfWeek = DayOfWeek.Tuesday, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0) 
                }
            }
        };
        
        var expectedSchedules = new List<ProviderScheduleResponseDto>
        {
            new ProviderScheduleResponseDto 
            { 
                Id = 1, 
                ProviderId = ProviderId, 
                DayOfWeek = DayOfWeek.Monday, 
                StartTime = new TimeSpan(9, 0, 0), 
                EndTime = new TimeSpan(18, 0, 0) 
            },
            new ProviderScheduleResponseDto 
            { 
                Id = 2, 
                ProviderId = ProviderId, 
                DayOfWeek = DayOfWeek.Tuesday, 
                StartTime = new TimeSpan(9, 0, 0), 
                EndTime = new TimeSpan(18, 0, 0) 
            }
        };
        
        _mockScheduleService
            .Setup(s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        var result = await _sut.BulkUpdateMySchedules(bulkUpdateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSchedules);
        
        _mockScheduleService.Verify(
            s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpdateMySchedules_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var bulkUpdateDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto 
                { 
                    DayOfWeek = DayOfWeek.Monday, 
                    StartTime = new TimeSpan(18, 0, 0), 
                    EndTime = new TimeSpan(9, 0, 0) // Invalid: end before start
                }
            }
        };
        
        _mockScheduleService
            .Setup(s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto))
            .ReturnsAsync(Result.Fail(new BadRequestError("La hora de fin debe ser posterior a la de inicio")));

        // Act
        var result = await _sut.BulkUpdateMySchedules(bulkUpdateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockScheduleService.Verify(
            s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpdate_WithValidProviderId_ReturnsOkWithUpdatedSchedules()
    {
        // Arrange
        var targetProviderId = 10;
        var bulkUpdateDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto 
                { 
                    DayOfWeek = DayOfWeek.Friday, 
                    StartTime = new TimeSpan(10, 0, 0), 
                    EndTime = new TimeSpan(15, 0, 0) 
                }
            }
        };
        
        var expectedSchedules = new List<ProviderScheduleResponseDto>
        {
            new ProviderScheduleResponseDto 
            { 
                Id = 1, 
                ProviderId = targetProviderId, 
                DayOfWeek = DayOfWeek.Friday, 
                StartTime = new TimeSpan(10, 0, 0), 
                EndTime = new TimeSpan(15, 0, 0) 
            }
        };
        
        _mockScheduleService
            .Setup(s => s.BulkUpdateAsync(BusinessId, targetProviderId, bulkUpdateDto))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        var result = await _sut.BulkUpdate(targetProviderId, bulkUpdateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSchedules);
        
        _mockScheduleService.Verify(
            s => s.BulkUpdateAsync(BusinessId, targetProviderId, bulkUpdateDto),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpdate_WhenProviderNotFound_ReturnsNotFound()
    {
        // Arrange
        var targetProviderId = 999;
        var bulkUpdateDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>()
        };
        
        _mockScheduleService
            .Setup(s => s.BulkUpdateAsync(BusinessId, targetProviderId, bulkUpdateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Proveedor no encontrado")));

        // Act
        var result = await _sut.BulkUpdate(targetProviderId, bulkUpdateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockScheduleService.Verify(
            s => s.BulkUpdateAsync(BusinessId, targetProviderId, bulkUpdateDto),
            Times.Once);
    }

    [Fact]
    public async Task GetMySchedules_UsesProviderIdFromClaims()
    {
        // Arrange
        var expectedSchedules = new List<ProviderScheduleResponseDto>();
        
        _mockScheduleService
            .Setup(s => s.GetByProviderAsync(BusinessId, ProviderId))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        await _sut.GetMySchedules();

        // Assert
        _mockScheduleService.Verify(
            s => s.GetByProviderAsync(BusinessId, ProviderId),
            Times.Once,
            "Should use ProviderId from authenticated user claims");
    }

    [Fact]
    public async Task BulkUpdateMySchedules_UsesProviderIdFromClaims()
    {
        // Arrange
        var bulkUpdateDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>()
        };
        
        var expectedSchedules = new List<ProviderScheduleResponseDto>();
        
        _mockScheduleService
            .Setup(s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto))
            .ReturnsAsync(Result.Ok<IEnumerable<ProviderScheduleResponseDto>>(expectedSchedules));

        // Act
        await _sut.BulkUpdateMySchedules(bulkUpdateDto);

        // Assert
        _mockScheduleService.Verify(
            s => s.BulkUpdateAsync(BusinessId, ProviderId, bulkUpdateDto),
            Times.Once,
            "Should use ProviderId from authenticated user claims");
    }
}


