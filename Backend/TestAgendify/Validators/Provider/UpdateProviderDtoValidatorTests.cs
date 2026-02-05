using Agendify.DTOs.Provider;
using Agendify.Validators.Provider;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Provider;

public class UpdateProviderDtoValidatorTests
{
    private readonly UpdateProviderDtoValidator _validator;

    public UpdateProviderDtoValidatorTests()
    {
        _validator = new UpdateProviderDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = string.Empty,
            Specialty = "Barbero"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = new string('A', 201),
            Specialty = "Barbero"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = "Barbero"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Specialty Tests

    [Fact]
    public void Should_Have_Error_When_Specialty_Is_Empty()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Specialty)
            .WithErrorMessage("La especialidad es requerida");
    }

    [Fact]
    public void Should_Have_Error_When_Specialty_Exceeds_MaxLength()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = new string('A', 201)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Specialty)
            .WithErrorMessage("La especialidad no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Specialty_Is_Valid()
    {
        var dto = new UpdateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = "Barbero profesional"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Specialty);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var dto = new UpdateProviderDto
        {
            Name = "Juan Pérez",
            Specialty = "Barbero y estilista"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Are_Invalid()
    {
        var dto = new UpdateProviderDto
        {
            Name = string.Empty,
            Specialty = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Specialty);
    }

    #endregion
}

