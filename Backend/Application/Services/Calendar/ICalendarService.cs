using Agendify.API.DTOs.Appointment;

namespace Agendify.API.Services.Calendar;

public interface ICalendarService
{
    Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(int businessId, DateTime startDate, DateTime endDate);
}
