using Agendify.DTOs.Calendar;
using Agendify.Services.Appointments;
using Agendify.Services.Providers;
using Agendify.Services.ProviderSchedules;

namespace Agendify.Services.Calendar.Summary;

/// <summary>
/// Servicio especializado para calcular resúmenes de calendario por rango de fechas
/// Responsabilidad: agregar y calcular métricas diarias (appointments count, minutos programados/ocupados/disponibles)
/// </summary>
public class CalendarSummaryService : ICalendarSummaryService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IProviderService _providerService;
    private readonly IProviderScheduleService _scheduleService;

    public CalendarSummaryService(
        IAppointmentService appointmentService,
        IProviderService providerService,
        IProviderScheduleService scheduleService)
    {
        _appointmentService = appointmentService;
        _providerService = providerService;
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Obtiene resumen diario del calendario para un rango de fechas con minutos disponibles históricos
    /// </summary>
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

        // ✅ OPTIMIZACIÓN: Calcular UNA sola vez y pasar como parámetro
        var appointmentsByDate = await CalculateOccupiedMinutesByDateAsync(businessId, startDate, endDate);
        var schedulesByDate = await GetSchedulesByDateRangeAsync(providerIds, startDate, endDate);

        // Generar summaries con los datos ya calculados
        return GenerateDailySummaries(startDate, endDate, schedulesByDate, appointmentsByDate);
    }

    /// <summary>
    /// Calcula minutos ocupados por appointments agrupados por fecha
    /// </summary>
    /// <returns>Dictionary con fecha como key y tupla (cantidad appointments, minutos totales)</returns>
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

    /// <summary>
    /// Obtiene minutos programados por fecha usando versionado temporal (1 query + filtrado en memoria)
    /// </summary>
    /// <returns>Dictionary con DateTime como key y minutos totales programados como value</returns>
    /// <remarks>
    /// 1. Hace 1 query que trae TODOS los schedules del rango
    /// 2. Filtra en memoria cuáles eran válidos para cada fecha específica
    /// 3. Optimizado: 1 query vs N queries (30x más rápido para 30 días)
    /// </remarks>
    private async Task<Dictionary<DateTime, int>> GetSchedulesByDateRangeAsync(
        List<int> providerIds,
        DateTime startDate,
        DateTime endDate)
    {
        // Traer TODOS los schedules que intersectan con el rango de fechas
        var allSchedules = await _scheduleService.GetSchedulesForDateRangeAsync(
            providerIds, startDate, endDate);

        // Agrupar minutos por fecha
        var schedulesByDate = new Dictionary<DateTime, int>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;

            // Filtrar schedules válidos para esta fecha específica
            var totalMinutes = allSchedules
                .Where(s =>
                    s.ValidFrom.Date <= currentDate &&
                    (s.ValidUntil == null || s.ValidUntil.Value.Date >= currentDate) &&
                    s.DayOfWeek == dayOfWeek)
                .Sum(s => (int)(s.EndTime - s.StartTime).TotalMinutes);

            schedulesByDate[currentDate] = totalMinutes;
            currentDate = currentDate.AddDays(1);
        }

        return schedulesByDate;
    }

    /// <summary>
    /// Genera la lista de summaries diarios combinando schedules y appointments
    /// </summary>
    private static List<CalendarDaySummaryDto> GenerateDailySummaries(
        DateTime startDate,
        DateTime endDate,
        Dictionary<DateTime, int> schedulesByDate,
        Dictionary<DateTime, (int Count, int Minutes)> appointmentsByDate)
    {
        var summaries = new List<CalendarDaySummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var totalScheduledMinutes = schedulesByDate.GetValueOrDefault(currentDate, 0);
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

    /// <summary>
    /// Genera días vacíos (todos en 0) cuando no hay providers en el business
    /// </summary>
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

    /// <summary>
    /// Obtiene IDs de todos los providers activos del business
    /// </summary>
    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        return await _providerService.GetProviderIdsByBusinessAsync(businessId);
    }
}

