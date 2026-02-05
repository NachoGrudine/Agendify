using Agendify.DTOs.Service;
using Agendify.Validators.Service;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Service;

public class UpdateServiceDtoValidatorTests
{
    private readonly UpdateServiceDtoValidator _validator;

    public UpdateServiceDtoValidatorTests()
    {
        _validator = new UpdateServiceDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = string.Empty,
            DefaultDuration = 30,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = new string('A', 201),
            DefaultDuration = 30,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region DefaultDuration Tests

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Is_Zero()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 0,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration)
            .WithErrorMessage("La duración debe ser mayor a 0 minutos");
    }

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Is_Negative()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = -10,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration);
    }

    [Fact]
    public void Should_Have_Error_When_DefaultDuration_Exceeds_1440()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 1441,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration)
            .WithErrorMessage("La duración no puede exceder 1440 minutos (24 horas)");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(1440)]
    public void Should_Not_Have_Error_When_DefaultDuration_Is_Valid(int duration)
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = duration,
            Price = 100
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.DefaultDuration);
    }

    #endregion

    #region Price Tests

    [Fact]
    public void Should_Have_Error_When_Price_Is_Negative()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = -10
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("El precio no puede ser negativo");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Exceeds_Maximum()
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = 1000000
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("El precio no puede exceder 999,999");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(999999)]
    public void Should_Not_Have_Error_When_Price_Is_Valid(decimal price)
    {
        var dto = new UpdateServiceDto 
        { 
            Name = "Corte de pelo",
            DefaultDuration = 30,
            Price = price
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var dto = new UpdateServiceDto
        {
            Name = "Corte de pelo y barba",
            DefaultDuration = 45,
            Price = 250.50m
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Are_Invalid()
    {
        var dto = new UpdateServiceDto
        {
            Name = string.Empty,
            DefaultDuration = -1,
            Price = -10
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.DefaultDuration);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    #endregion
}

