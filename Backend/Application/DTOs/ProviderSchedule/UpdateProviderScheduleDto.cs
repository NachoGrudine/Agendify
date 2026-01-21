namespace Agendify.API.DTOs.ProviderSchedule;

public class UpdateProviderScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

