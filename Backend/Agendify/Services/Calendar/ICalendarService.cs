﻿﻿using Agendify.DTOs.Calendar;

namespace Agendify.Services.Calendar;

public interface ICalendarService
{
    Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId, 
        DateTime date,
        int page = 1,
        int pageSize = 10,
        string? startTimeFrom = null,
        string? startTimeTo = null,
        string? searchText = null);
}
