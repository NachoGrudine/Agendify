using Agendify.DTOs.Service;
using Agendify.Validators.Service;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Service;

public class CreateServiceDtoValidatorTests
{
    private readonly CreateServiceDtoValidator _validator;

    public CreateServiceDtoValidatorTests()
    {
        _validator = new CreateServiceDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = string.Empty,
            DefaultDuration = 30,
            Price = 100
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
        var dto = new CreateServiceDto 
        { 
            Name = null!,
            DefaultDuration = 30,
            Price = 100
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
        var dto = new CreateServiceDto 
        { 
            Name = new string('A', 201),
            DefaultDuration = 30,
            Price = 100
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
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = 100
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
        var dto = new CreateServiceDto 
        { 
            Name = new string('A', 200),
            DefaultDuration = 30,
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region DefaultDuration Tests

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Is_Zero()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 0,
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration)
            .WithErrorMessage("La duración debe ser mayor a 0 minutos");
    }

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Is_Negative()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = -10,
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration)
            .WithErrorMessage("La duración debe ser mayor a 0 minutos");
    }

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Exceeds_1440_Minutes()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 1441,
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration)
            .WithErrorMessage("La duración no puede exceder 1440 minutos (24 horas)");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(1440)]
    public void Should_Not_Have_Error_When_DefaultDuration_Is_Valid(int duration)
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = duration,
            Price = 100
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DefaultDuration);
    }

    #endregion

    #region Price Tests

    [Fact]
    public void Should_Have_Error_When_Price_Is_Negative()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = -10
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("El precio no puede ser negativo");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Exceeds_Maximum()
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = 1000000
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("El precio no puede exceder 999,999");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(999999)]
    public void Should_Not_Have_Error_When_Price_Is_Valid(decimal price)
    {
        // Arrange
        var dto = new CreateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = price
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new CreateServiceDto
        {
            Name = "Corte de pelo y barba",
            DefaultDuration = 45,
            Price = 250.50m
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
        var dto = new CreateServiceDto
        {
            Name = string.Empty,
            DefaultDuration = -1,
            Price = -10
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Price_Is_Zero()
    {
        // Arrange
        var dto = new CreateServiceDto
        {
            Name = "Servicio gratuito",
            DefaultDuration = 30,
            Price = 0
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Not_Have_Error_When_DefaultDuration_Is_Exactly_1440()
    {
        // Arrange
        var dto = new CreateServiceDto
        {
            Name = "Servicio de 24 horas",
            DefaultDuration = 1440,
            Price = 1000
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DefaultDuration);
    }

    #endregion
}

