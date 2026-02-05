using Agendify.DTOs.ProviderSchedule;
using Agendify.Validators.ProviderSchedule;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.ProviderSchedule;

public class BulkUpdateProviderSchedulesDtoValidatorTests
{
    private readonly BulkUpdateProviderSchedulesDtoValidator _validator;

    public BulkUpdateProviderSchedulesDtoValidatorTests()
    {
        _validator = new BulkUpdateProviderSchedulesDtoValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Schedules_Is_Null()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = null!
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Schedules)
            .WithErrorMessage("La lista de horarios es requerida");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Schedules_Is_Empty_List()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Schedules);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Schedule_Items_Are_Valid()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto
                {
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(17)
                },
                new ProviderScheduleItemDto
                {
                    DayOfWeek = DayOfWeek.Tuesday,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(17)
                }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Schedule_Item_Has_Invalid_DayOfWeek()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto
                {
                    DayOfWeek = (DayOfWeek)999,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(17)
                }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Schedules[0].DayOfWeek");
    }

    [Fact]
    public void Should_Have_Error_When_Schedule_Item_Has_Invalid_Times()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto
                {
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = TimeSpan.FromHours(17),
                    EndTime = TimeSpan.FromHours(9)
                }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Schedules[0].EndTime");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Schedule_Items_Are_Invalid()
    {
        var dto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new ProviderScheduleItemDto
                {
                    DayOfWeek = (DayOfWeek)999,
                    StartTime = TimeSpan.FromHours(-1),
                    EndTime = TimeSpan.FromHours(25)
                },
                new ProviderScheduleItemDto
                {
                    DayOfWeek = (DayOfWeek)888,
                    StartTime = TimeSpan.FromHours(25),
                    EndTime = TimeSpan.FromHours(9)
                }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Schedules[0].DayOfWeek");
        result.ShouldHaveValidationErrorFor("Schedules[0].StartTime");
        result.ShouldHaveValidationErrorFor("Schedules[1].DayOfWeek");
    }
}

