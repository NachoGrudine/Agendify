﻿using Agendify.DTOs.Calendar;

namespace Agendify.Services.Calendar;

public interface ICalendarService
{
    Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId, 
        DateTime date, 
        string? status = null, 
        string? startTime = null, 
        string? customerName = null, 
        string? providerName = null);
}
