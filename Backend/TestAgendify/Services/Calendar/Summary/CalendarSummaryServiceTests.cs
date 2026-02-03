using Agendify.DTOs.Calendar;
using Agendify.Models.Entities;
using Agendify.Services.Appointments;
using Agendify.Services.Calendar.Summary;
using Agendify.Services.Providers;
using Agendify.Services.ProviderSchedules;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.Calendar.Summary;

public class CalendarSummaryServiceTests
{
    private readonly Mock<IAppointmentService> _mockAppointmentService;
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly Mock<IProviderScheduleService> _mockScheduleService;
    private readonly CalendarSummaryService _sut;

    public CalendarSummaryServiceTests()
    {
        _mockAppointmentService = new Mock<IAppointmentService>();
        _mockProviderService = new Mock<IProviderService>();
        _mockScheduleService = new Mock<IProviderScheduleService>();
        _sut = new CalendarSummaryService(
            _mockAppointmentService.Object,
            _mockProviderService.Object,
            _mockScheduleService.Object
        );
    }
    

    [Fact]
    public async Task GetCalendarSummaryAsync_WhenNoProviders_ShouldReturnEmptyDays()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 3);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int>());

        // Act
        var result = (await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(day =>
        {
            day.AppointmentsCount.Should().Be(0);
            day.TotalScheduledMinutes.Should().Be(0);
            day.TotalOccupiedMinutes.Should().Be(0);
            day.TotalAvailableMinutes.Should().Be(0);
        });

        _mockAppointmentService.Verify(
            x => x.GetAppointmentsWithDetailsByDateRangeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetCalendarSummaryAsync_WhenNoAppointments_ShouldShowFullAvailability()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 2); // Lunes
        var endDate = new DateTime(2026, 2, 2);

        var providerIds = new List<int> { 1 };

        var schedules = new List<ProviderSchedule>
        {
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17), // 480 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            }
        };

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(providerIds);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, startDate, endDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockScheduleService
            .Setup(x => x.GetSchedulesForDateRangeAsync(providerIds, startDate, endDate))
            .ReturnsAsync(schedules);

        // Act
        var result = (await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate)).ToList();

        // Assert
        result.Should().ContainSingle();

        var day = result[0];
        day.AppointmentsCount.Should().Be(0);
        day.TotalScheduledMinutes.Should().Be(480);
        day.TotalOccupiedMinutes.Should().Be(0);
        day.TotalAvailableMinutes.Should().Be(480);
    }
    [Fact]
    public async Task GetCalendarSummaryAsync_ShouldNormalizeDates()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1, 15, 30, 45); // Con hora específica
        var endDate = new DateTime(2026, 2, 3, 23, 59, 59);

        var expectedStartDate = new DateTime(2026, 2, 1); // Normalizado
        var expectedEndDate = new DateTime(2026, 2, 3);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int>());

        // Act
        var result = (await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Date.Should().Be(expectedStartDate);
        result[2].Date.Should().Be(expectedEndDate);
    }

    [Fact]
    public async Task GetCalendarSummaryAsync_WhenOccupiedExceedsScheduled_ShouldSetAvailableToZero()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 2); // Lunes
        var endDate = new DateTime(2026, 2, 2);

        var providerIds = new List<int> { 1 };

        // Más appointments que tiempo programado (caso edge: ej. appointments creados antes de ajustar schedule)
        var appointments = new List<Appointment>
        {
            new() { StartTime = new DateTime(2026, 2, 2, 9, 0, 0), EndTime = new DateTime(2026, 2, 2, 18, 0, 0) } // 540 min
        };

        var schedules = new List<ProviderSchedule>
        {
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17), // 480 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            }
        };

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(providerIds);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, startDate, endDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockScheduleService
            .Setup(x => x.GetSchedulesForDateRangeAsync(providerIds, startDate, endDate))
            .ReturnsAsync(schedules);

        // Act
        var result = (await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate)).ToList();

        // Assert
        result.Should().ContainSingle();

        var day = result[0];
        day.TotalScheduledMinutes.Should().Be(480);
        day.TotalOccupiedMinutes.Should().Be(540);
        day.TotalAvailableMinutes.Should().Be(0); // No negativo, sino 0
    }

    [Fact]
    public async Task GetCalendarSummaryAsync_WithLongDateRange_ShouldGenerateAllDays()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 28); // Todo febrero

        var providerIds = new List<int> { 1 };

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(providerIds);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, startDate, endDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockScheduleService
            .Setup(x => x.GetSchedulesForDateRangeAsync(providerIds, startDate, endDate))
            .ReturnsAsync(new List<ProviderSchedule>());

        // Act
        var result = (await _sut.GetCalendarSummaryAsync(businessId, startDate, endDate)).ToList();

        // Assert
        result.Should().HaveCount(28); // 28 días de febrero 2026
        result.First().Date.Should().Be(new DateTime(2026, 2, 1));
        result.Last().Date.Should().Be(new DateTime(2026, 2, 28));
    }


}

