using Agendify.Controllers;
using Agendify.DTOs.Calendar;
using Agendify.Services.Calendar;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Agendify.DTOs.Appointment;

namespace TestAgendify.Controllers;

public class CalendarControllerTests
{
    private readonly Mock<ICalendarService> _mockCalendarService;
    private readonly CalendarController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public CalendarControllerTests()
    {
        _mockCalendarService = new Mock<ICalendarService>();
        _sut = new CalendarController(_mockCalendarService.Object);
        
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
    public async Task GetCalendarSummary_WithValidDateRange_ReturnsOkWithSummary()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 7);
        var expectedSummary = new List<CalendarDaySummaryDto>
        {
            new CalendarDaySummaryDto
            {
                Date = new DateTime(2026, 1, 1),
                AppointmentsCount = 5,
                TotalScheduledMinutes = 480,
                TotalOccupiedMinutes = 300,
                TotalAvailableMinutes = 180
            },
            new CalendarDaySummaryDto
            {
                Date = new DateTime(2026, 1, 2),
                AppointmentsCount = 3,
                TotalScheduledMinutes = 480,
                TotalOccupiedMinutes = 180,
                TotalAvailableMinutes = 300
            }
        };
        
        _mockCalendarService
            .Setup(s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _sut.GetCalendarSummary(startDate, endDate);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSummary);
        
        _mockCalendarService.Verify(
            s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarSummary_WithEmptyResults_ReturnsOkWithEmptyList()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 7);
        var expectedSummary = new List<CalendarDaySummaryDto>();
        
        _mockCalendarService
            .Setup(s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _sut.GetCalendarSummary(startDate, endDate);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSummary);
        
        _mockCalendarService.Verify(
            s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetDayDetails_WithValidDate_ReturnsOkWithDetails()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = 10,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = 1,
            PageSize = 15,
            TotalPages = 1
        };
        
        _mockCalendarService
            .Setup(s => s.GetDayDetailsAsync(
                BusinessId, 
                date, 
                1, 
                15, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetails(date);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedDetails);
        
        _mockCalendarService.Verify(
            s => s.GetDayDetailsAsync(BusinessId, date, 1, 15, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetDayDetails_WithPagination_ReturnsOkWithPaginatedResults()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        var page = 2;
        var pageSize = 10;
        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = 25,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = 3
        };
        
        _mockCalendarService
            .Setup(s => s.GetDayDetailsAsync(
                BusinessId, 
                date, 
                page, 
                pageSize, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetails(date, page, pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedDetails);
        
        _mockCalendarService.Verify(
            s => s.GetDayDetailsAsync(BusinessId, date, page, pageSize, null, null),
            Times.Once);
    }

    [Fact]
    public async Task GetDayDetails_WithSearchText_ReturnsOkWithFilteredResults()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        var searchText = "John";
        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = 2,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = 1,
            PageSize = 15,
            TotalPages = 1
        };
        
        _mockCalendarService
            .Setup(s => s.GetDayDetailsAsync(
                BusinessId, date, 1, 15, null, searchText))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetails(date, 1, 15, null, searchText);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedDetails);
        
        _mockCalendarService.Verify(
            s => s.GetDayDetailsAsync(BusinessId, date, 1, 15, null, searchText),
            Times.Once);
    }

    [Fact]
    public async Task GetDayDetails_WithAllFilters_ReturnsOkWithFilteredResults()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        var page = 1;
        var pageSize = 20;
        var startTime = "09:00";
        var searchText = "consultation";
        
        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = 1,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = 1
        };
        
        _mockCalendarService
            .Setup(s => s.GetDayDetailsAsync(
                BusinessId, 
                date, 
                page, 
                pageSize, 
                startTime, 
                searchText))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetails(date, page, pageSize, startTime, searchText);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedDetails);
        
        _mockCalendarService.Verify(
            s => s.GetDayDetailsAsync(BusinessId, date, page, pageSize, startTime, searchText),
            Times.Once);
    }

    [Fact]
    public async Task GetCalendarSummary_UsesBusinessIdFromClaims()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 7);
        var expectedSummary = new List<CalendarDaySummaryDto>();
        
        _mockCalendarService
            .Setup(s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate))
            .ReturnsAsync(expectedSummary);

        // Act
        await _sut.GetCalendarSummary(startDate, endDate);

        // Assert
        _mockCalendarService.Verify(
            s => s.GetCalendarSummaryAsync(BusinessId, startDate, endDate),
            Times.Once,
            "Should use BusinessId from authenticated user claims");
    }

    [Fact]
    public async Task GetDayDetails_UsesBusinessIdFromClaims()
    {
        // Arrange
        var date = new DateTime(2026, 1, 15);
        var expectedDetails = new DayDetailsDto();
        
        _mockCalendarService
            .Setup(s => s.GetDayDetailsAsync(BusinessId, date, 1, 15, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        await _sut.GetDayDetails(date);

        // Assert
        _mockCalendarService.Verify(
            s => s.GetDayDetailsAsync(BusinessId, date, 1, 15, null, null),
            Times.Once,
            "Should use BusinessId from authenticated user claims");
    }
}



