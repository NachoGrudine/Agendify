namespace Agendify.DTOs.ProviderSchedule;

public class BulkUpdateProviderSchedulesDto
{
    public List<ProviderScheduleItemDto> Schedules { get; set; } = new();
}

public class ProviderScheduleItemDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

