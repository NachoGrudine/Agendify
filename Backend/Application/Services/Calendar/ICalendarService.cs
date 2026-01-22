﻿using Agendify.API.DTOs.Calendar;

namespace Agendify.API.Services.Calendar;

public interface ICalendarService
{
    Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<DayDetailsDto> GetDayDetailsAsync(int businessId, DateTime date);
}
