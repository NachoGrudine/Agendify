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

    /// <summary>
    /// Obtiene resumen diario del calendario para un rango de fechas con minutos disponibles históricos
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="startDate">Fecha inicio del rango</param>
    /// <param name="endDate">Fecha fin del rango</param>
    /// <returns>Lista de summaries diarios con appointments count, minutos programados, ocupados y disponibles</returns>
    /// <remarks>Usa versionado temporal: cada fecha obtiene schedules que existían ESE día (1 query optimizada)</remarks>
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
    /// Obtiene detalles completos de un día específico con appointments y métricas de disponibilidad
    /// </summary>
    public async Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId, 
        DateTime date,
        int page = 1,
        int pageSize = 10,
        string? status = null, 
        string? startTime = null, 
        string? searchText = null)
    {
        date = date.Date;
        ValidatePaginationParams(ref page, ref pageSize);

        // 1. Obtener todos los appointments del día
        var allAppointments = await GetAllAppointmentsForDateAsync(businessId, date);

        // 2. Calcular totales del día completo (sin filtros)
        var (totalAppointments, totalOccupiedMinutes) = CalculateDayTotals(allAppointments);

        // 3. Aplicar filtros de búsqueda
        var filteredAppointments = ApplyFilters(allAppointments, status, startTime, searchText);

        // 4. Aplicar paginación
        var (paginatedAppointments, totalCount, totalPages) = ApplyPagination(filteredAppointments, page, pageSize);

        // 5. Calcular minutos programados del día
        var totalScheduledMinutes = await GetScheduledMinutesForDateAsync(businessId, date);

        // 6. Calcular tendencia de appointments (comparar con día anterior)
        var appointmentsTrend = await _appointmentService.GetAppointmentsTrendAsync(businessId, date);

        // 7. Mapear a DTOs
        var appointmentDetails = paginatedAppointments.Select(MapToAppointmentDetailDto).ToList();

        return new DayDetailsDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            TotalAppointments = totalAppointments,
            AppointmentsTrend = appointmentsTrend,
            TotalScheduledMinutes = totalScheduledMinutes,
            TotalOccupiedMinutes = totalOccupiedMinutes,
            Appointments = appointmentDetails,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Valida y corrige parámetros de paginación
    /// </summary>
    private static void ValidatePaginationParams(ref int page, ref int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
    }

    /// <summary>
    /// Obtiene todos los appointments de un día específico
    /// </summary>
    private async Task<List<Appointment>> GetAllAppointmentsForDateAsync(int businessId, DateTime date)
    {
        var appointments = await _appointmentService.GetAppointmentsWithDetailsByDateRangeAsync(
            businessId, date, date.AddDays(1).AddSeconds(-1));

        return appointments.ToList();
    }

    /// <summary>
    /// Calcula totales del día: cantidad de turnos y minutos ocupados
    /// </summary>
    private static (int TotalAppointments, int TotalOccupiedMinutes) CalculateDayTotals(List<Appointment> appointments)
    {
        var totalAppointments = appointments.Count;
        var totalOccupiedMinutes = appointments.Sum(a => (int)(a.EndTime - a.StartTime).TotalMinutes);
        
        return (totalAppointments, totalOccupiedMinutes);
    }

    /// <summary>
    /// Aplica filtros de búsqueda a la lista de appointments
    /// </summary>
    private static List<Appointment> ApplyFilters(
        List<Appointment> appointments, 
        string? status, 
        string? startTime, 
        string? searchText)
    {
        var filtered = appointments.OrderByDescending(a => a.StartTime).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered
                .Where(a => a.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(startTime))
        {
            filtered = filtered
                .Where(a => a.StartTime.ToString("HH:mm") == startTime)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered
                .Where(a =>
                    (a.Customer != null && a.Customer.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Service != null && a.Service.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Provider != null && a.Provider.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return filtered;
    }

    /// <summary>
    /// Aplica paginación a la lista de appointments
    /// </summary>
    private static (List<Appointment> PaginatedAppointments, int TotalCount, int TotalPages) ApplyPagination(
        List<Appointment> appointments, 
        int page, 
        int pageSize)
    {
        var totalCount = appointments.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var paginated = appointments
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (paginated, totalCount, totalPages);
    }

    /// <summary>
    /// Obtiene los minutos programados para una fecha específica
    /// </summary>
    private async Task<int> GetScheduledMinutesForDateAsync(int businessId, DateTime date)
    {
        var providerIds = await GetProviderIdsAsync(businessId);
        var minutesByDayOfWeek = await _scheduleService.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date);
        
        return minutesByDayOfWeek.GetValueOrDefault(date.DayOfWeek, 0);
    }

    /// <summary>
    /// Mapea entidad Appointment a DTO de detalle con formato de hora (HH:mm)
    /// </summary>
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

    /// <summary>
    /// Obtiene IDs de todos los providers activos del business
    /// </summary>
    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        return await _providerService.GetProviderIdsByBusinessAsync(businessId);
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
}

