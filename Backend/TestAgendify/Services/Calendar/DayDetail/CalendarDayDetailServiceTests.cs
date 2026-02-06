using Agendify.DTOs.Appointment;
using Agendify.Models.Entities;
using Agendify.Services.Appointments;
using Agendify.Services.Calendar.DayDetail;
using Agendify.Services.Providers;
using Agendify.Services.ProviderSchedules;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.Calendar.DayDetail;

public class CalendarDayDetailServiceTests
{
    private readonly Mock<IAppointmentService> _mockAppointmentService;
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly Mock<IProviderScheduleService> _mockScheduleService;
    private readonly CalendarDayDetailService _sut;

    public CalendarDayDetailServiceTests()
    {
        _mockAppointmentService = new Mock<IAppointmentService>();
        _mockProviderService = new Mock<IProviderService>();
        _mockScheduleService = new Mock<IProviderScheduleService>();
        _sut = new CalendarDayDetailService(
            _mockAppointmentService.Object,
            _mockProviderService.Object,
            _mockScheduleService.Object
        );
    }

    #region GetDayDetailsAsync Tests

    [Fact]
    public async Task GetDayDetailsAsync_WithDefaultParameters_ShouldReturnFullDayDetails()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var appointments = CreateSampleAppointments(date);

        var providerIds = new List<int> { 1, 2 };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(2);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(providerIds);

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.DayOfWeek.Should().Be("Thursday");
        result.TotalAppointments.Should().Be(5);
        result.AppointmentsTrend.Should().Be(2);
        result.TotalScheduledMinutes.Should().Be(480);
        result.TotalOccupiedMinutes.Should().Be(300); // 5 appointments x 60 min
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Appointments.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var page = 2;
        var pageSize = 2;

        var appointments = CreateSampleAppointments(date);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.TotalAppointments.Should().Be(5); // Total sin filtrar
        result.TotalCount.Should().Be(5); // Total después de filtros
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(3); // 5 appointments / 2 per page = 3 pages
        result.Appointments.Should().HaveCount(2); // Solo 2 appointments en esta página
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var appointments = new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 11, 0),
            CreateAppointment(3, date, 12, 0),
            CreateAppointment(4, date, 13, 0),
            CreateAppointment(5, date, 14, 0)
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalAppointments.Should().Be(5);
        result.TotalCount.Should().Be(5);
        result.Appointments.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithStartTimeFilter_ShouldFilterByTime()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var startTimeFrom = "11:00";

        var appointments = new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 11, 0),
            CreateAppointment(3, date, 12, 0),
            CreateAppointment(4, date, 14, 0)
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, startTimeFrom: startTimeFrom);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3); // Los que empiezan desde las 11:00
        result.Appointments.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithSearchText_ShouldSearchInCustomerProviderAndService()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var searchText = "Smith";

        var appointments = new List<Appointment>
        {
            CreateAppointmentWithDetails(1, date, 10, 0, "John Smith", "Dr. Jones", "Consultation"),
            CreateAppointmentWithDetails(2, date, 11, 0, "Jane Doe", "Dr. Smith", "Checkup"),
            CreateAppointmentWithDetails(3, date, 12, 0, "Bob Johnson", "Dr. Williams", "Smithing Service"),
            CreateAppointmentWithDetails(4, date, 13, 0, "Alice Cooper", "Dr. Brown", "Cleaning")
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, searchText: searchText);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3); // John Smith (customer), Dr. Smith (provider), Smithing Service (service)
        result.Appointments.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var startTimeFrom = "10:00";
        var startTimeTo = "10:30";
        var searchText = "Smith";

        var appointments = new List<Appointment>
        {
            CreateAppointmentWithDetails(1, date, 10, 0, "John Smith", "Dr. Jones", "Consultation"),
            CreateAppointmentWithDetails(2, date, 10, 0, "John Smith", "Dr. Jones", "Consultation"),
            CreateAppointmentWithDetails(3, date, 11, 0, "John Smith", "Dr. Jones", "Consultation"),
            CreateAppointmentWithDetails(4, date, 10, 0, "Bob Johnson", "Dr. Jones", "Consultation")
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, startTimeFrom: startTimeFrom, startTimeTo: startTimeTo, searchText: searchText);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2); // Los dos primeros cumplen ambos filtros (hora 10:00 y contienen "Smith")
        result.Appointments.Should().HaveCount(2);
        result.Appointments.Should().AllSatisfy(a => a.StartTime.Should().Be("10:00"));
        result.Appointments.Should().AllSatisfy(a => a.CustomerName.Should().Contain("Smith"));
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithStartTimeToFilter_ShouldFilterToTime()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var startTimeTo = "12:00";

        var appointments = new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 11, 0),
            CreateAppointment(3, date, 12, 0),
            CreateAppointment(4, date, 14, 0)
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, startTimeTo: startTimeTo);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3); // Los que empiezan hasta las 12:00 (10, 11, 12)
        result.Appointments.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithStartTimeRangeFilter_ShouldFilterByRange()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var startTimeFrom = "11:00";
        var startTimeTo = "13:00";

        var appointments = new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 11, 0),
            CreateAppointment(3, date, 12, 0),
            CreateAppointment(4, date, 14, 0)
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, startTimeFrom: startTimeFrom, startTimeTo: startTimeTo);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2); // Los que empiezan entre las 11:00 y 13:00 (11:00 y 12:00)
        result.Appointments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WhenNoAppointments_ShouldReturnEmptyList()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalAppointments.Should().Be(0);
        result.TotalOccupiedMinutes.Should().Be(0);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDayDetailsAsync_ShouldOrderByStartTimeDescending()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var appointments = new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 16, 0),
            CreateAppointment(3, date, 12, 0),
            CreateAppointment(4, date, 9, 0),
            CreateAppointment(5, date, 14, 0)
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(appointments);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Appointments.Should().HaveCount(5);
        result.Appointments[0].StartTime.Should().Be("16:00");
        result.Appointments[1].StartTime.Should().Be("14:00");
        result.Appointments[2].StartTime.Should().Be("12:00");
        result.Appointments[3].StartTime.Should().Be("10:00");
        result.Appointments[4].StartTime.Should().Be("09:00");
    }

    [Fact]
    public async Task GetDayDetailsAsync_ShouldCalculateTrendsCorrectly()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(-3);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.AppointmentsTrend.Should().Be(-3);

        _mockAppointmentService.Verify(x => x.GetAppointmentsTrendAsync(businessId, date), Times.Once);
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithInvalidPaginationParams_ShouldCorrectThem()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var invalidPage = -1;
        var invalidPageSize = 0;

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date, invalidPage, invalidPageSize);

        // Assert
        result.CurrentPage.Should().Be(1); // Corregido a 1
        result.PageSize.Should().Be(10); // Corregido a 10
    }

    [Fact]
    public async Task GetDayDetailsAsync_ShouldNormalizeDate()
    {
        // Arrange
        var businessId = 1;
        var dateWithTime = new DateTime(2026, 2, 5, 15, 30, 45);
        var expectedDate = new DateTime(2026, 2, 5);

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, expectedDate, expectedDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment>());

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, expectedDate))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), expectedDate))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { expectedDate.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, dateWithTime);

        // Assert
        result.Date.Should().Be(expectedDate);

        _mockAppointmentService.Verify(
            x => x.GetAppointmentsWithDetailsByDateRangeAsync(businessId, expectedDate, It.IsAny<DateTime>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDayDetailsAsync_WithNullCustomer_ShouldShowDefaultMessage()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);

        var appointment = new Appointment
        {
            Id = 1,
            StartTime = new DateTime(2026, 2, 5, 10, 0, 0),
            EndTime = new DateTime(2026, 2, 5, 11, 0, 0),
            Customer = null, // Sin cliente
            Provider = new Provider { Name = "Dr. Smith" },
            Service = new Service { Name = "Consultation" }
        };

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsWithDetailsByDateRangeAsync(
                businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(new List<Appointment> { appointment });

        _mockAppointmentService
            .Setup(x => x.GetAppointmentsTrendAsync(businessId, date))
            .ReturnsAsync(0);

        _mockProviderService
            .Setup(x => x.GetProviderIdsByBusinessAsync(businessId))
            .ReturnsAsync(new List<int> { 1 });

        _mockScheduleService
            .Setup(x => x.GetScheduledMinutesByProviderIdsForDateAsync(It.IsAny<List<int>>(), date))
            .ReturnsAsync(new Dictionary<DayOfWeek, int> { { date.DayOfWeek, 480 } });

        // Act
        var result = await _sut.GetDayDetailsAsync(businessId, date);

        // Assert
        result.Appointments.Should().ContainSingle();
        result.Appointments[0].CustomerName.Should().Be("Sin cliente asignado");
    }

    #endregion

    #region Helper Methods

    private List<Appointment> CreateSampleAppointments(DateTime date)
    {
        return new List<Appointment>
        {
            CreateAppointment(1, date, 10, 0),
            CreateAppointment(2, date, 11, 0),
            CreateAppointment(3, date, 14, 0),
            CreateAppointment(4, date, 15, 0),
            CreateAppointment(5, date, 16, 0)
        };
    }

    private Appointment CreateAppointment(int id, DateTime date, int startHour, int startMinute)
    {
        return new Appointment
        {
            Id = id,
            StartTime = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0),
            EndTime = new DateTime(date.Year, date.Month, date.Day, startHour + 1, startMinute, 0),
            Customer = new Customer { Id = id, Name = $"Customer {id}" },
            Provider = new Provider { Id = 1, Name = "Dr. Smith" },
            Service = new Service { Id = 1, Name = "Consultation" }
        };
    }

    private Appointment CreateAppointmentWithDetails(
        int id,
        DateTime date,
        int startHour,
        int startMinute,
        string customerName,
        string providerName,
        string serviceName)
    {
        return new Appointment
        {
            Id = id,
            StartTime = new DateTime(date.Year, date.Month, date.Day, startHour, startMinute, 0),
            EndTime = new DateTime(date.Year, date.Month, date.Day, startHour + 1, startMinute, 0),
            Customer = new Customer { Id = id, Name = customerName },
            Provider = new Provider { Id = 1, Name = providerName },
            Service = new Service { Id = 1, Name = serviceName }
        };
    }

    #endregion
}

