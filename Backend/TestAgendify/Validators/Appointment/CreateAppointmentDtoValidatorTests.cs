using Agendify.DTOs.Appointment;
using Agendify.Validators.Appointment;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Appointment;

public class CreateAppointmentDtoValidatorTests
{
    private readonly CreateAppointmentDtoValidator _validator;

    public CreateAppointmentDtoValidatorTests()
    {
        _validator = new CreateAppointmentDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Zero()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 0,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
            .WithErrorMessage("El ID del proveedor es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Negative()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = -1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProviderId_Is_Valid()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProviderId);
    }

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_Empty()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = default,
            EndTime = DateTime.Now.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Empty()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = default
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_After_EndTime()
    {
        // Arrange
        var now = DateTime.Now;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now.AddHours(2),
            EndTime = now
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("La fecha de inicio debe ser anterior a la fecha de fin");
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Before_StartTime()
    {
        // Arrange
        var now = DateTime.Now;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(-1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La fecha de fin debe ser posterior a la fecha de inicio");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Times_Are_Valid()
    {
        // Arrange
        var now = DateTime.Now;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Zero()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("El ID del cliente debe ser mayor a 0 si se proporciona");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Negative()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = -1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Valid()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = 1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Null()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Have_Error_When_ServiceId_Is_Zero()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceId)
            .WithErrorMessage("El ID del servicio debe ser mayor a 0 si se proporciona");
    }

    [Fact]
    public void Should_Have_Error_When_ServiceId_Is_Negative()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = -1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ServiceId_Is_Valid()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = 1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ServiceId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ServiceId_Is_Null()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ServiceId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerName_Is_Null()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerName = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ServiceName_Is_Null()
    {
        // Arrange
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceName = null
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ServiceName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var now = DateTime.Now;
        var dto = new CreateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(1),
            CustomerId = 1,
            CustomerName = "John Doe",
            ServiceId = 1,
            ServiceName = "Corte de pelo",
            Notes = "Cliente nuevo"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

