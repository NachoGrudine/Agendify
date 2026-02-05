using Agendify.DTOs.Provider;
using Agendify.Validators.Provider;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Provider;

public class CreateProviderDtoValidatorTests
{
    private readonly CreateProviderDtoValidator _validator;

    public CreateProviderDtoValidatorTests()
    {
        _validator = new CreateProviderDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = string.Empty,
            Specialty = "Barbero"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = null!,
            Specialty = "Barbero"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = new string('A', 201),
            Specialty = "Barbero"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = "Barbero"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Has_Exactly_200_Characters()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = new string('A', 200),
            Specialty = "Barbero"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Specialty Tests

    [Fact]
    public void Should_Have_Error_When_Specialty_Is_Empty()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = string.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Specialty)
            .WithErrorMessage("La especialidad es requerida");
    }

    [Fact]
    public void Should_Have_Error_When_Specialty_Is_Null()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = null!
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Specialty);
    }

    [Fact]
    public void Should_Have_Error_When_Specialty_Exceeds_MaxLength()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = new string('A', 201)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Specialty)
            .WithErrorMessage("La especialidad no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Specialty_Is_Valid()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = "Barbero profesional"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Specialty);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Specialty_Has_Exactly_200_Characters()
    {
        // Arrange
        var dto = new CreateProviderDto 
        { 
            Name = "Juan Pérez",
            Specialty = new string('A', 200)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Specialty);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new CreateProviderDto
        {
            Name = "Juan Pérez",
            Specialty = "Barbero y estilista"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Are_Invalid()
    {
        // Arrange
        var dto = new CreateProviderDto
        {
            Name = string.Empty,
            Specialty = string.Empty
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Specialty);
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Fields_Exceed_MaxLength()
    {
        // Arrange
        var dto = new CreateProviderDto
        {
            Name = new string('A', 201),
            Specialty = new string('B', 201)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Specialty);
    }

    #endregion
}

