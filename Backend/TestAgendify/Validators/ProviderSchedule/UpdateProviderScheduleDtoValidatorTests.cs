using Agendify.DTOs.ProviderSchedule;
using Agendify.Validators.ProviderSchedule;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.ProviderSchedule;

public class UpdateProviderScheduleDtoValidatorTests
{
    private readonly UpdateProviderScheduleDtoValidator _validator;

    public UpdateProviderScheduleDtoValidatorTests()
    {
        _validator = new UpdateProviderScheduleDtoValidator();
    }

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
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = day,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.DayOfWeek);
    }

    [Fact]
    public void Should_Have_Error_When_DayOfWeek_Is_Invalid()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = (DayOfWeek)999,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DayOfWeek)
            .WithErrorMessage("El día de la semana no es válido");
    }

    #endregion

    #region StartTime Tests

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_Negative()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(-1),
            EndTime = TimeSpan.FromHours(17)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartTime)
            .WithErrorMessage("La hora de inicio debe estar entre 00:00 y 23:59");
    }

    [Fact]
    public void Should_Have_Error_When_StartTime_Is_24_Hours_Or_More()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(24),
            EndTime = TimeSpan.FromHours(25)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }
    

    #endregion

    #region EndTime Tests

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Negative()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(-1)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Greater_Than_24_Hours()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(25)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Not_Have_Error_When_EndTime_Is_Exactly_24_Hours()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(0),
            EndTime = TimeSpan.FromHours(24)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Is_Not_Greater_Than_StartTime()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(17),
            EndTime = TimeSpan.FromHours(9)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("La hora de fin debe ser posterior a la hora de inicio");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = DayOfWeek.Monday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Fields_Are_Invalid()
    {
        var dto = new UpdateProviderScheduleDto
        {
            DayOfWeek = (DayOfWeek)999,
            StartTime = TimeSpan.FromHours(-1),
            EndTime = TimeSpan.FromHours(25)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DayOfWeek);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    #endregion
}

