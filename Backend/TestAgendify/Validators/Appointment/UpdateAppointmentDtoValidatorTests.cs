using Agendify.DTOs.Appointment;
using Agendify.Validators.Appointment;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Appointment;

public class UpdateAppointmentDtoValidatorTests
{
    private readonly UpdateAppointmentDtoValidator _validator;

    public UpdateAppointmentDtoValidatorTests()
    {
        _validator = new UpdateAppointmentDtoValidator();
    }

    #region ProviderId Tests

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Zero()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 0,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProviderId)
            .WithErrorMessage("El ID del proveedor es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Negative()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = -1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProviderId_Is_Valid()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ProviderId);
    }

    #endregion

    #region Time Validation Tests

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_After_EndTime()
    {
        var now = DateTime.Now;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now.AddHours(2),
            EndTime = now
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("La fecha de inicio debe ser anterior a la fecha de fin");
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Before_StartTime()
    {
        var now = DateTime.Now;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(-1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La fecha de fin debe ser posterior a la fecha de inicio");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Times_Are_Valid()
    {
        var now = DateTime.Now;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    #endregion

    #region CustomerId Tests

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Zero()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = 0
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("El ID del cliente debe ser mayor a 0 si se proporciona");
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Valid()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_CustomerId_Is_Null()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            CustomerId = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    #endregion

    #region ServiceId Tests

    [Fact]
    public void Should_Have_Error_When_ServiceId_Is_Zero()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = 0
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ServiceId)
            .WithErrorMessage("El ID del servicio debe ser mayor a 0 si se proporciona");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ServiceId_Is_Valid()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ServiceId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ServiceId_Is_Null()
    {
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1),
            ServiceId = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ServiceId);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var now = DateTime.Now;
        var dto = new UpdateAppointmentDto
        {
            ProviderId = 1,
            StartTime = now,
            EndTime = now.AddHours(1),
            CustomerId = 1,
            CustomerName = "John Doe",
            ServiceId = 1,
            ServiceName = "Corte de pelo",
            Notes = "Cliente frecuente"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}

