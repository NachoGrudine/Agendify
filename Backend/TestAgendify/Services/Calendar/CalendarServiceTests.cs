using Agendify.DTOs.Appointment;
using Agendify.DTOs.Calendar;
using Agendify.Services.Calendar;
using Agendify.Services.Calendar.DayDetail;
using Agendify.Services.Calendar.Summary;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.Calendar;

public class CalendarServiceTests
{
    private readonly Mock<ICalendarSummaryService> _mockSummaryService;
    private readonly Mock<ICalendarDayDetailService> _mockDayDetailService;
    private readonly CalendarService _sut;

    public CalendarServiceTests()
    {
        _mockSummaryService = new Mock<ICalendarSummaryService>();
        _mockDayDetailService = new Mock<ICalendarDayDetailService>();
        _sut = new CalendarService(_mockSummaryService.Object, _mockDayDetailService.Object);
    }

    #region GetCalendarSummaryAsync Tests

    [Fact]
    public async Task GetCalendarSummaryAsync_ShouldDelegateToSummaryService()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 7);

        var expectedSummaries = new List<CalendarDaySummaryDto>
        {
            new()
            {
                Date = new DateTime(2026, 2, 1),
                AppointmentsCount = 5,
                TotalScheduledMinutes = 480,
                TotalOccupiedMinutes = 300,
                TotalAvailableMinutes = 180
            },
            new()
            {
                Date = new DateTime(2026, 2, 2),
                AppointmentsCount = 3,
                TotalScheduledMinutes = 480,
                TotalOccupiedMinutes = 180,
                TotalAvailableMinutes = 300
            }
        };

        _mockSummaryService
            .Setup(x => x.GetCalendarSummaryAsync(businessId, startDate, endDate))
            .ReturnsAsync(expectedSummaries);

        // Act
        var result = await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedSummaries);

        _mockSummaryService.Verify(
            x => x.GetCalendarSummaryAsync(businessId, startDate, endDate),
            Times.Once
        );
    }

    [Fact]
    public async Task GetCalendarSummaryAsync_WhenNoData_ShouldReturnEmptyCollection()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 7);

        _mockSummaryService
            .Setup(x => x.GetCalendarSummaryAsync(businessId, startDate, endDate))
            .ReturnsAsync(new List<CalendarDaySummaryDto>());

        // Act
        var result = await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCalendarSummaryAsync_ShouldPassCorrectParameters()
    {
        // Arrange
        var businessId = 42;
        var startDate = new DateTime(2026, 3, 15);
        var endDate = new DateTime(2026, 3, 31);

        _mockSummaryService
            .Setup(x => x.GetCalendarSummaryAsync(businessId, startDate, endDate))
            .ReturnsAsync(new List<CalendarDaySummaryDto>());

        // Act
        await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate);

        // Assert
        _mockSummaryService.Verify(
            x => x.GetCalendarSummaryAsync(
                It.Is<int>(id => id == 42),
                It.Is<DateTime>(d => d == startDate),
                It.Is<DateTime>(d => d == endDate)),
            Times.Once
        );
    }

    #endregion

    #region GetDayDetailsAsync Tests

    [Fact]
    public async Task GetDayDetailsAsync_WithDefaultParameters_ShouldDelegateToDetailService()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = "Wednesday",
            TotalAppointments = 8,
            AppointmentsTrend = 2,
            TotalScheduledMinutes = 480,
            TotalOccupiedMinutes = 360,
            Appointments = new List<AppointmentDetailDto>
            {
                new()
                {
                    Id = 1,
                    CustomerName = "John Doe",
                    ProviderName = "Dr. Smith",
                    ServiceName = "Consultation",
                    StartTime = "10:00",
                    EndTime = "11:00",
                    DurationMinutes = 60,
                }
            },
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1,
            TotalCount = 8
        };

        _mockDayDetailService
            .Setup(x => x.GetDayDetailsAsync(businessId, date, 1, 10, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDetails);

        _mockDayDetailService.Verify(
            x => x.GetDayDetailsAsync(businessId, date, 1, 10, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithAllFilters_ShouldPassAllParameters()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var page = 2;
        var pageSize = 20;
        var startTime = "10:00";
        var searchText = "John";

        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = "Wednesday",
            TotalAppointments = 25,
            AppointmentsTrend = -1,
            TotalScheduledMinutes = 480,
            TotalOccupiedMinutes = 420,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = 2,
            TotalCount = 25
        };

        _mockDayDetailService
            .Setup(x => x.GetDayDetailsAsync(businessId, date, page, pageSize, startTime, searchText))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, page, pageSize, startTime, searchText);

        // Assert
        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(25);

        _mockDayDetailService.Verify(
            x => x.GetDayDetailsAsync(
                It.Is<int>(id => id == businessId),
                It.Is<DateTime>(d => d == date),
                It.Is<int>(p => p == page),
                It.Is<int>(ps => ps == pageSize),
                It.Is<string>(st => st == startTime),
                It.Is<string>(txt => txt == searchText)),
            Times.Once
        );
    }


    [Fact]
    public async Task GetDayDetailsAsync_WithSearchText_ShouldSearchCorrectly()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var searchText = "Smith";

        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = "Wednesday",
            TotalAppointments = 2,
            Appointments = new List<AppointmentDetailDto>
            {
                new()
                {
                    Id = 1,
                    CustomerName = "John Smith",
                    ProviderName = "Dr. Jones",
                    StartTime = "10:00",
                    EndTime = "11:00",
                    DurationMinutes = 60,
                },
                new()
                {
                    Id = 2,
                    CustomerName = "Mary Johnson",
                    ProviderName = "Dr. Smith",
                    StartTime = "14:00",
                    EndTime = "15:00",
                    DurationMinutes = 60,
                }
            },
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1,
            TotalCount = 2
        };

        _mockDayDetailService
            .Setup(x => x.GetDayDetailsAsync(businessId, date, 1, 10, null, searchText))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, searchText: searchText);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Appointments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithPagination_ShouldPaginateCorrectly()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var page = 3;
        var pageSize = 5;

        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = "Wednesday",
            TotalAppointments = 25,
            Appointments = new List<AppointmentDetailDto>
            {
                new() { Id = 11, CustomerName = "Customer 11", ProviderName = "Dr. A", StartTime = "10:00", EndTime = "11:00", DurationMinutes = 60 },
                new() { Id = 12, CustomerName = "Customer 12", ProviderName = "Dr. B", StartTime = "11:00", EndTime = "12:00", DurationMinutes = 60 },
                new() { Id = 13, CustomerName = "Customer 13", ProviderName = "Dr. C", StartTime = "12:00", EndTime = "13:00", DurationMinutes = 60 },
                new() { Id = 14, CustomerName = "Customer 14", ProviderName = "Dr. D", StartTime = "13:00", EndTime = "14:00", DurationMinutes = 60 },
                new() { Id = 15, CustomerName = "Customer 15", ProviderName = "Dr. E", StartTime = "14:00", EndTime = "15:00", DurationMinutes = 60 }
            },
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = 5,
            TotalCount = 25
        };

        _mockDayDetailService
            .Setup(x => x.GetDayDetailsAsync(businessId, date, page, pageSize, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.CurrentPage.Should().Be(3);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(5);
        result.TotalCount.Should().Be(25);
        result.Appointments.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WhenNoAppointments_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var expectedDetails = new DayDetailsDto
        {
            Date = date,
            DayOfWeek = "Wednesday",
            TotalAppointments = 0,
            AppointmentsTrend = 0,
            TotalScheduledMinutes = 480,
            TotalOccupiedMinutes = 0,
            Appointments = new List<AppointmentDetailDto>(),
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 0,
            TotalCount = 0
        };

        _mockDayDetailService
            .Setup(x => x.GetDayDetailsAsync(businessId, date, 1, 10, null, null))
            .ReturnsAsync(expectedDetails);

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalAppointments.Should().Be(0);
        result.Appointments.Should().BeEmpty();
    }

    #endregion
}



