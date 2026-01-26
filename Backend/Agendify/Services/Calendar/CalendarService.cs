using Agendify.Models.Entities;
using Agendify.DTOs.Appointment;
using Agendify.DTOs.Calendar;
using Agendify.Services.Appointments;
using Agendify.Services.Providers;
using Agendify.Services.ProviderSchedules;

namespace Agendify.Services.Calendar;

public class CalendarService : ICalendarService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IProviderService _providerService;
    private readonly IProviderScheduleService _scheduleService;

    public CalendarService(
        IAppointmentService appointmentService,
        IProviderService providerService,
        IProviderScheduleService scheduleService)
    {
        _appointmentService = appointmentService;
        _providerService = providerService;
        _scheduleService = scheduleService;
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

    public async Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId, 
        DateTime date, 
        string? status = null, 
        string? startTime = null, 
        string? customerName = null, 
        string? providerName = null)
    {
        date = date.Date;

        // 1. Obtener todos los appointments de ese día
        var appointments = await _appointmentService.GetAppointmentsWithDetailsByDateRangeAsync(
            businessId, date, date.AddDays(1).AddSeconds(-1));

        var appointmentsList = appointments
            .OrderByDescending(a => a.StartTime)
            .ToList();

        // 2. Aplicar filtros
        if (!string.IsNullOrWhiteSpace(status))
        {
            appointmentsList = appointmentsList
                .Where(a => a.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(startTime))
        {
            appointmentsList = appointmentsList
                .Where(a => a.StartTime.ToString("HH:mm") == startTime)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            appointmentsList = appointmentsList
                .Where(a => a.Customer != null && 
                           a.Customer.Name.Contains(customerName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(providerName))
        {
            appointmentsList = appointmentsList
                .Where(a => a.Provider != null && 
                           a.Provider.Name.Contains(providerName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // 3. Calcular minutos programados para ese día de la semana
        var providerIds = await GetProviderIdsAsync(businessId);
        var minutesByDayOfWeek = await CalculateScheduledMinutesByDayOfWeekAsync(providerIds);
        var totalScheduledMinutes = minutesByDayOfWeek.GetValueOrDefault(date.DayOfWeek, 0);

        // 4. Calcular minutos ocupados (solo de los appointments filtrados)
        var totalOccupiedMinutes = appointmentsList.Sum(a => (int)(a.EndTime - a.StartTime).TotalMinutes);

        // 5. Mapear appointments a DTOs
        var appointmentDetails = appointmentsList.Select(MapToAppointmentDetailDto).ToList();

        return new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = appointmentsList.Count,
            TotalScheduledMinutes = totalScheduledMinutes,
            TotalOccupiedMinutes = totalOccupiedMinutes,
            Appointments = appointmentDetails
        };
    }

    private static AppointmentDetailDto MapToAppointmentDetailDto(Appointment appointment)
    {
        var customer = appointment.Customer;
        var customerName = customer?.Name ?? "Sin cliente asignado";

        return new AppointmentDetailDto
        {
            Id = appointment.Id,
            CustomerName = customerName,
            ProviderName = appointment.Provider?.Name ?? "Sin proveedor",
            ServiceName = appointment.Service?.Name,
            StartTime = appointment.StartTime.ToString("HH:mm"),
            EndTime = appointment.EndTime.ToString("HH:mm"),
            DurationMinutes = (int)(appointment.EndTime - appointment.StartTime).TotalMinutes,
            Status = appointment.Status.ToString()
        };
    }

    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        return await _providerService.GetProviderIdsByBusinessAsync(businessId);
    }

    private async Task<Dictionary<DayOfWeek, int>> CalculateScheduledMinutesByDayOfWeekAsync(List<int> providerIds)
    {
        return await _scheduleService.GetScheduledMinutesByProviderIdsAsync(providerIds);
    }

    private async Task<Dictionary<DateTime, (int Count, int Minutes)>> CalculateOccupiedMinutesByDateAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        var appointments = await _appointmentService.GetAppointmentsWithDetailsByDateRangeAsync(
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
        DateTime startDate, //rango
        DateTime endDate,   // rango
        Dictionary<DayOfWeek, int> minutesByDayOfWeek, // tiempo programado para trabajar
        Dictionary<DateTime, (int Count, int Minutes)> appointmentsByDate) // tiempo consumido
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

