using Agendify.Controllers;
using Agendify.DTOs.Appointment;
using Agendify.Services.Appointments;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Agendify.Common.Errors;

namespace TestAgendify.Controllers;

public class AppointmentsControllerTests
{
    private readonly Mock<IAppointmentService> _mockAppointmentService;
    private readonly AppointmentsController _sut;
    private const int BusinessId = 1;
    private const int UserId = 100;

    public AppointmentsControllerTests()
    {
        _mockAppointmentService = new Mock<IAppointmentService>();
        _sut = new AppointmentsController(_mockAppointmentService.Object);
        
        // Setup HttpContext and User Claims for authentication
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
    public async Task GetById_WhenAppointmentExists_ReturnsOkWithAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var expectedResponse = new AppointmentResponseDto
        {
            Id = appointmentId,
            BusinessId = BusinessId,
            ProviderId = 1,
            ProviderName = "Dr. Smith",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
        };
        
        _mockAppointmentService
            .Setup(s => s.GetByIdAsync(BusinessId, appointmentId))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.GetById(appointmentId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockAppointmentService.Verify(
            s => s.GetByIdAsync(BusinessId, appointmentId),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WhenAppointmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = 999;
        _mockAppointmentService
            .Setup(s => s.GetByIdAsync(BusinessId, appointmentId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Turno no encontrado")));

        // Act
        var result = await _sut.GetById(appointmentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockAppointmentService.Verify(
            s => s.GetByIdAsync(BusinessId, appointmentId),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateAppointmentDto
        {
            ProviderId = 1,
            CustomerId = 1,
            ServiceId = 1,
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Notes = "Test appointment"
        };
        
        var expectedResponse = new AppointmentResponseDto
        {
            Id = 1,
            BusinessId = BusinessId,
            ProviderId = createDto.ProviderId,
            CustomerId = createDto.CustomerId,
            ServiceId = createDto.ServiceId,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Notes = createDto.Notes
        };
        
        _mockAppointmentService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.ActionName.Should().Be(nameof(_sut.GetById));
        
        _mockAppointmentService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(-1) // Invalid: end before start
        };
        
        _mockAppointmentService
            .Setup(s => s.CreateAsync(BusinessId, createDto))
            .ReturnsAsync(Result.Fail("La hora de fin debe ser posterior a la de inicio"));

        // Act
        var result = await _sut.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        _mockAppointmentService.Verify(
            s => s.CreateAsync(BusinessId, createDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var updateDto = new UpdateAppointmentDto
        {
            ProviderId = 2,
            StartTime = DateTime.Now.AddDays(2),
            EndTime = DateTime.Now.AddDays(2).AddHours(1)
        };
        
        var expectedResponse = new AppointmentResponseDto
        {
            Id = appointmentId,
            BusinessId = BusinessId,
            ProviderId = updateDto.ProviderId,
            StartTime = updateDto.StartTime,
            EndTime = updateDto.EndTime
        };
        
        _mockAppointmentService
            .Setup(s => s.UpdateAsync(BusinessId, appointmentId, updateDto))
            .ReturnsAsync(Result.Ok(expectedResponse));

        // Act
        var result = await _sut.Update(appointmentId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockAppointmentService.Verify(
            s => s.UpdateAsync(BusinessId, appointmentId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenAppointmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = 999;
        var updateDto = new UpdateAppointmentDto
        {
            ProviderId = 2,
            StartTime = DateTime.Now.AddDays(2),
            EndTime = DateTime.Now.AddDays(2).AddHours(1)
        };
        
        _mockAppointmentService
            .Setup(s => s.UpdateAsync(BusinessId, appointmentId, updateDto))
            .ReturnsAsync(Result.Fail(new NotFoundError("Turno no encontrado")));

        // Act
        var result = await _sut.Update(appointmentId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockAppointmentService.Verify(
            s => s.UpdateAsync(BusinessId, appointmentId, updateDto),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenAppointmentExists_ReturnsNoContent()
    {
        // Arrange
        var appointmentId = 1;
        _mockAppointmentService
            .Setup(s => s.DeleteAsync(BusinessId, appointmentId))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _sut.Delete(appointmentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        _mockAppointmentService.Verify(
            s => s.DeleteAsync(BusinessId, appointmentId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenAppointmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = 999;
        _mockAppointmentService
            .Setup(s => s.DeleteAsync(BusinessId, appointmentId))
            .ReturnsAsync(Result.Fail(new NotFoundError("Turno no encontrado")));

        // Act
        var result = await _sut.Delete(appointmentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        
        _mockAppointmentService.Verify(
            s => s.DeleteAsync(BusinessId, appointmentId),
            Times.Once);
    }
}

