﻿namespace Agendify.DTOs.ProviderSchedule;

public class ProviderScheduleResponseDto
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

