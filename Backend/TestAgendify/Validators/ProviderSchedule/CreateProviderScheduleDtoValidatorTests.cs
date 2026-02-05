using Agendify.DTOs.ProviderSchedule;
using Agendify.Validators.ProviderSchedule;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.ProviderSchedule;

public class CreateProviderScheduleDtoValidatorTests
{
    private readonly CreateProviderScheduleDtoValidator _validator;

    public CreateProviderScheduleDtoValidatorTests()
    {
        _validator = new CreateProviderScheduleDtoValidator();
    }

    #region ProviderId Tests

    [Fact]
    public void Should_Have_Error_When_ProviderId_Is_Zero()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 0,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
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
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = -1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
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
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProviderId);
    }

    #endregion

    #region DayOfWeek Tests

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    public void Should_Not_Have_Error_When_DayOfWeek_Is_Valid(DayOfWeek day)
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = day,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DayOfWeek);
    }

    [Fact]
    public void Should_Have_Error_When_DayOfWeek_Is_Invalid()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = (DayOfWeek)999, // Invalid enum value
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DayOfWeek)
            .WithErrorMessage("El día de la semana no es válido");
    }

    #endregion

    #region StartTime Tests


    [Fact]
    public void Should_Have_Error_When_StartTime_Is_Negative()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(-1),
            EndTime = TimeSpan.FromHours(17)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("La hora de inicio debe estar entre 00:00 y 23:59");
    }

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_24_Hours_Or_More()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(24),
            EndTime = TimeSpan.FromHours(25)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("La hora de inicio debe estar entre 00:00 y 23:59");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(9)]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(23)]
    public void Should_Not_Have_Error_When_StartTime_Is_Valid(int hours)
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(hours),
            EndTime = TimeSpan.FromHours(hours + 1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Should_Not_Have_Error_When_StartTime_Is_Zero()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromHours(1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    #endregion

    #region EndTime Tests

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Zero()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.Zero
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Negative()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(-1)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La hora de fin debe estar entre 00:01 y 24:00");
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Greater_Than_24_Hours()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(25)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La hora de fin debe estar entre 00:01 y 24:00");
    }

    [Fact]
    public void Should_Not_Have_Error_When_EndTime_Is_Exactly_24_Hours()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(0),
            EndTime = TimeSpan.FromHours(24)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Not_Greater_Than_StartTime()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(17),
            EndTime = TimeSpan.FromHours(9)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La hora de fin debe ser posterior a la hora de inicio");
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Equals_StartTime()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(9)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La hora de fin debe ser posterior a la hora de inicio");
    }

    [Theory]
    [InlineData(9, 17)]
    [InlineData(6, 14)]
    [InlineData(18, 24)]
    public void Should_Not_Have_Error_When_Times_Are_Valid(int startHours, int endHours)
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(startHours),
            EndTime = TimeSpan.FromHours(endHours)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 0,
            DayOfWeek = (DayOfWeek)999,
            StartTime = TimeSpan.FromHours(-1),
            EndTime = TimeSpan.FromHours(25)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProviderId);
        result.ShouldHaveValidationErrorFor(x => x.DayOfWeek);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Schedule_Spans_Entire_Day()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromHours(24)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_With_Minutes_In_Times()
    {
        // Arrange
        var dto = new CreateProviderScheduleDto
        {
            ProviderId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 30, 0),
            EndTime = new TimeSpan(17, 45, 0)
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}

