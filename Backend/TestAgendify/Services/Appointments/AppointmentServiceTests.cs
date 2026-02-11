using Agendify.Common.Errors;
using Agendify.DTOs.Appointment;
using Agendify.Models.Entities;
using Agendify.Repositories;
using Agendify.Services.Appointments;
using Agendify.Services.Customers;
using Agendify.Services.ServicesServices;
using FluentAssertions;
using Moq;

namespace TestAgendify.Services.Appointments;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IServiceService> _mockServiceService;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockServiceService = new Mock<IServiceService>();
        _sut = new AppointmentService(
            _mockAppointmentRepository.Object,
            _mockCustomerService.Object,
            _mockServiceService.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenNoConflict_ShouldCreateAppointmentSuccessfully()
    {
        // Arrange
        var businessId = 1;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 10,
            CustomerId = 5,
            ServiceId = 3,
            StartTime = new DateTime(2026, 2, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 2, 10, 11, 0, 0),
            Notes = "Test appointment"
        };

        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime))
            .ReturnsAsync(false);

        _mockCustomerService
            .Setup(x => x.ResolveOrCreateAsync(businessId, dto.CustomerId, dto.CustomerName))
            .ReturnsAsync(5);

        _mockServiceService
            .Setup(x => x.ResolveOrCreateAsync(businessId, dto.ServiceId, dto.ServiceName, 60))
            .ReturnsAsync(3);

        var createdAppointment = new Appointment
        {
            Id = 100,
            BusinessId = businessId,
            ProviderId = dto.ProviderId,
            CustomerId = 5,
            ServiceId = 3,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,            Notes = dto.Notes,
            Provider = new Provider { Id = 10, Name = "Dr. Smith" }
        };

        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(createdAppointment);

        // Act
        var result = await _sut.CreateAsync(businessId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(100);
        result.Value.ProviderId.Should().Be(10);
        result.Value.ProviderName.Should().Be("Dr. Smith");
        _mockAppointmentRepository.Verify(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime), Times.Once);
        _mockAppointmentRepository.Verify(x => x.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenConflictExists_ShouldReturnConflictError()
    {
        // Arrange
        var businessId = 1;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 10,
            StartTime = new DateTime(2026, 2, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 2, 10, 11, 0, 0)
        };

        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(businessId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<ConflictError>();
        result.Errors[0].Message.Should().Be("El proveedor ya tiene un turno asignado en ese horario");

        _mockAppointmentRepository.Verify(x => x.AddAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNewCustomerName_ShouldResolveOrCreateCustomer()
    {
        // Arrange
        var businessId = 1;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 10,
            CustomerName = "New Customer",
            StartTime = new DateTime(2026, 2, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 2, 10, 11, 0, 0)
        };

        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime))
            .ReturnsAsync(false);

        _mockCustomerService
            .Setup(x => x.ResolveOrCreateAsync(businessId, null, "New Customer"))
            .ReturnsAsync(99);

        _mockServiceService
            .Setup(x => x.ResolveOrCreateAsync(businessId, null, null, 60))
            .ReturnsAsync((int?)null);

        var createdAppointment = new Appointment
        {
            Id = 100,
            BusinessId = businessId,
            ProviderId = dto.ProviderId,
            CustomerId = 99,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,            Provider = new Provider { Id = 10, Name = "Dr. Smith" },
            Customer = new Customer { Id = 99, Name = "New Customer" }
        };

        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(createdAppointment);

        // Act
        var result = await _sut.CreateAsync(businessId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerId.Should().Be(99);
        result.Value.CustomerName.Should().Be("New Customer");

        _mockCustomerService.Verify(x => x.ResolveOrCreateAsync(businessId, null, "New Customer"), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenAppointmentExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 100;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 10,
            CustomerId = 5,
            ServiceId = 3,
            StartTime = new DateTime(2026, 2, 15, 14, 0, 0), // Fecha futura
            EndTime = new DateTime(2026, 2, 15, 15, 0, 0),
            Notes = "Updated notes"
        };

        var existingAppointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = businessId,
            ProviderId = 10,
            StartTime = new DateTime(2026, 2, 12, 10, 0, 0), // Fecha futura
            EndTime = new DateTime(2026, 2, 12, 11, 0, 0)
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(existingAppointment);

        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime, appointmentId))
            .ReturnsAsync(false);

        _mockCustomerService
            .Setup(x => x.ResolveOrCreateAsync(businessId, dto.CustomerId, dto.CustomerName))
            .ReturnsAsync(5);

        _mockServiceService
            .Setup(x => x.ResolveOrCreateAsync(businessId, dto.ServiceId, dto.ServiceName, 60))
            .ReturnsAsync(3);

        var updatedAppointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = businessId,
            ProviderId = dto.ProviderId,
            CustomerId = 5,
            ServiceId = 3,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Notes = dto.Notes,
            Provider = new Provider { Id = 10, Name = "Dr. Smith" }
        };

        _mockAppointmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(updatedAppointment);

        // Act
        var result = await _sut.UpdateAsync(businessId, appointmentId, dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(appointmentId);        result.Value.Notes.Should().Be("Updated notes");

        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenAppointmentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 999;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 10,
            StartTime = new DateTime(2026, 2, 15, 14, 0, 0), // Fecha futura
            EndTime = new DateTime(2026, 2, 15, 15, 0, 0),
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.UpdateAsync(businessId, appointmentId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Be("Turno no encontrado");

        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenBusinessIdMismatch_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 100;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 10,
            StartTime = new DateTime(2026, 2, 15, 14, 0, 0), // Fecha futura
            EndTime = new DateTime(2026, 2, 15, 15, 0, 0),
        };

        var existingAppointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = 999, // Different business
            ProviderId = 10
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(existingAppointment);

        // Act
        var result = await _sut.UpdateAsync(businessId, appointmentId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task UpdateAsync_WhenConflictExists_ShouldReturnConflictError()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 100;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 10,
            StartTime = new DateTime(2026, 3, 15, 14, 0, 0), // Fecha futura en marzo
            EndTime = new DateTime(2026, 3, 15, 15, 0, 0),
        };

        var existingAppointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = businessId,
            ProviderId = 10
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(existingAppointment);

        _mockAppointmentRepository
            .Setup(x => x.HasConflictAsync(dto.ProviderId, dto.StartTime, dto.EndTime, appointmentId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(businessId, appointmentId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<ConflictError>();

        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentExists_ShouldReturnAppointment()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 100;

        var appointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = businessId,
            ProviderId = 10,
            StartTime = new DateTime(2026, 2, 10, 10, 0, 0),
            EndTime = new DateTime(2026, 2, 10, 11, 0, 0),
            Provider = new Provider { Id = 10, Name = "Dr. Smith" }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.GetByIdAsync(businessId, appointmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(appointmentId);
        result.Value.BusinessId.Should().Be(businessId);
        result.Value.ProviderName.Should().Be("Dr. Smith");
    }

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 999;

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.GetByIdAsync(businessId, appointmentId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
    }

    #endregion

    #region GetAppointmentsWithDetailsByDateRangeAsync Tests

    [Fact]
    public async Task GetAppointmentsWithDetailsByDateRangeAsync_ShouldReturnAppointmentsInRange()
    {
        // Arrange
        var businessId = 1;
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 28);

        var appointments = new List<Appointment>
        {
            new() { Id = 1, BusinessId = businessId, StartTime = new DateTime(2026, 2, 5, 10, 0, 0) },
            new() { Id = 2, BusinessId = businessId, StartTime = new DateTime(2026, 2, 10, 14, 0, 0) },
            new() { Id = 3, BusinessId = businessId, StartTime = new DateTime(2026, 2, 20, 9, 0, 0) }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, startDate, endDate))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.GetAppointmentsWithDetailsByDateRangeAsync(businessId, startDate, endDate);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Id == 1);
        result.Should().Contain(a => a.Id == 2);
        result.Should().Contain(a => a.Id == 3);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenAppointmentExists_ShouldMarkAsDeleted()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 100;

        var appointment = new Appointment
        {
            Id = appointmentId,
            BusinessId = businessId,
            IsDeleted = false
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        _mockAppointmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.DeleteAsync(businessId, appointmentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.IsDeleted.Should().BeTrue();

        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.Is<Appointment>(a => a.IsDeleted)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAppointmentNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var appointmentId = 999;

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.DeleteAsync(businessId, appointmentId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    #endregion

    #region GetAppointmentsTrendAsync Tests

    [Fact]
    public async Task GetAppointmentsTrendAsync_WhenMoreThanYesterday_ShouldReturnPositiveTrend()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var previousDate = new DateTime(2026, 2, 4);

        var todayAppointments = new List<Appointment>
        {
            new() { Id = 1, StartTime = new DateTime(2026, 2, 5, 10, 0, 0) },
            new() { Id = 2, StartTime = new DateTime(2026, 2, 5, 14, 0, 0) },
            new() { Id = 3, StartTime = new DateTime(2026, 2, 5, 16, 0, 0) }
        };

        var yesterdayAppointments = new List<Appointment>
        {
            new() { Id = 4, StartTime = new DateTime(2026, 2, 4, 10, 0, 0) }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(todayAppointments);

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, previousDate, previousDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(yesterdayAppointments);

        // Act
        var result = await _sut.GetAppointmentsTrendAsync(businessId, date);

        // Assert
        result.Should().Be(2); // 3 - 1 = +2
    }

    [Fact]
    public async Task GetAppointmentsTrendAsync_WhenLessThanYesterday_ShouldReturnNegativeTrend()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var previousDate = new DateTime(2026, 2, 4);

        var todayAppointments = new List<Appointment>
        {
            new() { Id = 1, StartTime = new DateTime(2026, 2, 5, 10, 0, 0) }
        };

        var yesterdayAppointments = new List<Appointment>
        {
            new() { Id = 2, StartTime = new DateTime(2026, 2, 4, 10, 0, 0) },
            new() { Id = 3, StartTime = new DateTime(2026, 2, 4, 14, 0, 0) },
            new() { Id = 4, StartTime = new DateTime(2026, 2, 4, 16, 0, 0) }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(todayAppointments);

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, previousDate, previousDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(yesterdayAppointments);

        // Act
        var result = await _sut.GetAppointmentsTrendAsync(businessId, date);

        // Assert
        result.Should().Be(-2); // 1 - 3 = -2
    }

    [Fact]
    public async Task GetAppointmentsTrendAsync_WhenSameAsYesterday_ShouldReturnZero()
    {
        // Arrange
        var businessId = 1;
        var date = new DateTime(2026, 2, 5);
        var previousDate = new DateTime(2026, 2, 4);

        var todayAppointments = new List<Appointment>
        {
            new() { Id = 1, StartTime = new DateTime(2026, 2, 5, 10, 0, 0) },
            new() { Id = 2, StartTime = new DateTime(2026, 2, 5, 14, 0, 0) }
        };

        var yesterdayAppointments = new List<Appointment>
        {
            new() { Id = 3, StartTime = new DateTime(2026, 2, 4, 10, 0, 0) },
            new() { Id = 4, StartTime = new DateTime(2026, 2, 4, 16, 0, 0) }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, date, date.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(todayAppointments);

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, previousDate, previousDate.AddDays(1).AddSeconds(-1)))
            .ReturnsAsync(yesterdayAppointments);

        // Act
        var result = await _sut.GetAppointmentsTrendAsync(businessId, date);

        // Assert
        result.Should().Be(0); // 2 - 2 = 0
    }

    [Fact]
    public async Task GetAppointmentsTrendAsync_ShouldNormalizeDate()
    {
        // Arrange
        var businessId = 1;
        var dateWithTime = new DateTime(2026, 2, 5, 15, 30, 45); // Con hora específica
        var expectedDate = new DateTime(2026, 2, 5); // Normalizado a medianoche

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, expectedDate, It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        _mockAppointmentRepository
            .Setup(x => x.GetByDateRangeAsync(businessId, expectedDate.AddDays(-1), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        await _sut.GetAppointmentsTrendAsync(businessId, dateWithTime);

        // Assert
        _mockAppointmentRepository.Verify(
            x => x.GetByDateRangeAsync(businessId, expectedDate, It.IsAny<DateTime>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task GetNextAppointmentAsync_WhenNextAppointmentExists_ShouldReturnNextAppointment()
    {
        // Arrange
        var businessId = 1;
        var currentDateTime = new DateTime(2026, 2, 5, 9, 0, 0);
        
        var nextAppointment = new Appointment
        {
            Id = 100,
            BusinessId = businessId,
            StartTime = new DateTime(2026, 2, 5, 14, 0, 0),
            EndTime = new DateTime(2026, 2, 5, 15, 0, 0),
            Customer = new Customer { Id = 5, Name = "Juan Pérez" }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetNextAppointmentAsync(businessId, currentDateTime))
            .ReturnsAsync(nextAppointment);

        // Act
        var result = await _sut.GetNextAppointmentAsync(businessId, currentDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerName.Should().Be("Juan Pérez");
        result.Value.StartTime.Should().Be(new DateTime(2026, 2, 5, 14, 0, 0));
        result.Value.EndTime.Should().Be(new DateTime(2026, 2, 5, 15, 0, 0));
        result.Value.Day.Should().Be(new DateTime(2026, 2, 5));
    }

    [Fact]
    public async Task GetNextAppointmentAsync_WhenNoUpcomingAppointments_ShouldReturnNotFoundError()
    {
        // Arrange
        var businessId = 1;
        var currentDateTime = new DateTime(2026, 2, 5, 9, 0, 0);

        _mockAppointmentRepository
            .Setup(x => x.GetNextAppointmentAsync(businessId, currentDateTime))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _sut.GetNextAppointmentAsync(businessId, currentDateTime);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<NotFoundError>();
        result.Errors[0].Message.Should().Be("No hay turnos próximos programados");
    }

    [Fact]
    public async Task GetNextAppointmentAsync_WhenCustomerIsNull_ShouldReturnDefaultCustomerName()
    {
        // Arrange
        var businessId = 1;
        var currentDateTime = new DateTime(2026, 2, 5, 9, 0, 0);
        
        var nextAppointment = new Appointment
        {
            Id = 100,
            BusinessId = businessId,
            StartTime = new DateTime(2026, 2, 5, 14, 0, 0),
            EndTime = new DateTime(2026, 2, 5, 15, 0, 0),
            Customer = null
        };

        _mockAppointmentRepository
            .Setup(x => x.GetNextAppointmentAsync(businessId, currentDateTime))
            .ReturnsAsync(nextAppointment);

        // Act
        var result = await _sut.GetNextAppointmentAsync(businessId, currentDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CustomerName.Should().Be("Sin cliente asignado");
    }

    [Fact]
    public async Task GetNextAppointmentAsync_ShouldReturnCorrectDay()
    {
        // Arrange
        var businessId = 1;
        var currentDateTime = new DateTime(2026, 2, 5, 23, 0, 0);
        
        var nextAppointment = new Appointment
        {
            Id = 100,
            BusinessId = businessId,
            StartTime = new DateTime(2026, 2, 6, 10, 30, 0), // Al día siguiente
            EndTime = new DateTime(2026, 2, 6, 11, 30, 0),
            Customer = new Customer { Id = 5, Name = "María López" }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetNextAppointmentAsync(businessId, currentDateTime))
            .ReturnsAsync(nextAppointment);

        // Act
        var result = await _sut.GetNextAppointmentAsync(businessId, currentDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Day.Should().Be(new DateTime(2026, 2, 6)); // Solo la fecha sin hora
        result.Value.StartTime.Hour.Should().Be(10);
        result.Value.StartTime.Minute.Should().Be(30);
    }

    #endregion
}


