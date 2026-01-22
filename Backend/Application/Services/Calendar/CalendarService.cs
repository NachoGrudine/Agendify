using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Appointment;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.Calendar;

public class CalendarService : ICalendarService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRepository<Provider> _providerRepository;
    private readonly IRepository<ProviderSchedule> _scheduleRepository;

    public CalendarService(
        IAppointmentRepository appointmentRepository,
        IRepository<Provider> providerRepository,
        IRepository<ProviderSchedule> scheduleRepository)
    {
        _appointmentRepository = appointmentRepository;
        _providerRepository = providerRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        var providerIds = await GetProviderIdsAsync(businessId);
        
        if (!providerIds.Any())
        {
            return GenerateEmptyDays(startDate, endDate);
        }

        var minutesByDayOfWeek = await CalculateScheduledMinutesByDayOfWeekAsync(providerIds);
        var appointmentsByDate = await CalculateOccupiedMinutesByDateAsync(businessId, startDate, endDate);

        return GenerateDailySummaries(startDate, endDate, minutesByDayOfWeek, appointmentsByDate);
    }

    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        var allProviders = await _providerRepository.FindAsync(p => p.BusinessId == businessId);
        return allProviders.Select(p => p.Id).ToList();
    }

    private async Task<Dictionary<DayOfWeek, int>> CalculateScheduledMinutesByDayOfWeekAsync(List<int> providerIds)
    {
        var allSchedules = await _scheduleRepository.FindAsync(s => providerIds.Contains(s.ProviderId));
        var minutesByDayOfWeek = new Dictionary<DayOfWeek, int>();

        foreach (var schedule in allSchedules)
        {
            var minutes = (int)(schedule.EndTime - schedule.StartTime).TotalMinutes;

            if (!minutesByDayOfWeek.ContainsKey(schedule.DayOfWeek))
            {
                minutesByDayOfWeek[schedule.DayOfWeek] = 0;
            }

            minutesByDayOfWeek[schedule.DayOfWeek] += minutes;
        }

        return minutesByDayOfWeek;
    }

    private async Task<Dictionary<DateTime, (int Count, int Minutes)>> CalculateOccupiedMinutesByDateAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        var appointments = await _appointmentRepository.GetByDateRangeAsync(
            businessId, startDate, endDate.AddDays(1).AddSeconds(-1));

        return appointments
            .GroupBy(a => a.StartTime.Date)
            .ToDictionary(
                g => g.Key,
                g => (
                    Count: g.Count(),
                    Minutes: g.Sum(a => (int)(a.EndTime - a.StartTime).TotalMinutes)
                )
            );
    }

    private static List<CalendarDaySummaryDto> GenerateDailySummaries(
        DateTime startDate,
        DateTime endDate,
        Dictionary<DayOfWeek, int> minutesByDayOfWeek,
        Dictionary<DateTime, (int Count, int Minutes)> appointmentsByDate)
    {
        var summaries = new List<CalendarDaySummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var totalScheduledMinutes = minutesByDayOfWeek.GetValueOrDefault(dayOfWeek, 0);
            var (appointmentsCount, totalOccupiedMinutes) = appointmentsByDate.GetValueOrDefault(currentDate, (0, 0));
            var totalAvailableMinutes = Math.Max(0, totalScheduledMinutes - totalOccupiedMinutes);

            summaries.Add(new CalendarDaySummaryDto
            {
                Date = currentDate,
                AppointmentsCount = appointmentsCount,
                TotalScheduledMinutes = totalScheduledMinutes,
                TotalOccupiedMinutes = totalOccupiedMinutes,
                TotalAvailableMinutes = totalAvailableMinutes
            });

            currentDate = currentDate.AddDays(1);
        }

        return summaries;
    }

    private static IEnumerable<CalendarDaySummaryDto> GenerateEmptyDays(DateTime startDate, DateTime endDate)
    {
        var summaries = new List<CalendarDaySummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            summaries.Add(new CalendarDaySummaryDto
            {
                Date = currentDate,
                AppointmentsCount = 0,
                TotalScheduledMinutes = 0,
                TotalOccupiedMinutes = 0,
                TotalAvailableMinutes = 0
            });

            currentDate = currentDate.AddDays(1);
        }

        return summaries;
    }
}

