using Agendify.DTOs.Appointment;
using Agendify.DTOs.Calendar;
using Agendify.Models.Entities;
using Agendify.Services.Appointments;
using Agendify.Services.Providers;
using Agendify.Services.ProviderSchedules;

namespace Agendify.Services.Calendar.DayDetail;

/// <summary>
/// Servicio especializado para obtener detalles completos de un día específico
/// Responsabilidad: filtrar, paginar y calcular métricas detalladas de appointments de un día
/// </summary>
public class CalendarDayDetailService : ICalendarDayDetailService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IProviderService _providerService;
    private readonly IProviderScheduleService _scheduleService;

    public CalendarDayDetailService(
        IAppointmentService appointmentService,
        IProviderService providerService,
        IProviderScheduleService scheduleService)
    {
        _appointmentService = appointmentService;
        _providerService = providerService;
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Obtiene detalles completos de un día específico con appointments y métricas de disponibilidad
    /// </summary>
    public async Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId,
        DateTime date,
        int page = 1,
        int pageSize = 10,
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
        var filteredAppointments = ApplyFilters(allAppointments, startTime, searchText);

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
    /// Obtiene todos los appointments de un día específico con relaciones cargadas
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
    /// Aplica filtros de búsqueda a la lista de appointments (hora, texto)
    /// </summary>
    private static List<Appointment> ApplyFilters(
        List<Appointment> appointments,
        string? startTime,
        string? searchText)
    {
        var filtered = appointments.OrderByDescending(a => a.StartTime).ToList();


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
    /// Obtiene los minutos programados (schedules) para una fecha específica
    /// </summary>
    private async Task<int> GetScheduledMinutesForDateAsync(int businessId, DateTime date)
    {
        var providerIds = await GetProviderIdsAsync(businessId);
        var minutesByDayOfWeek = await _scheduleService.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date);

        return minutesByDayOfWeek.GetValueOrDefault(date.DayOfWeek, 0);
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
            DurationMinutes = (int)(appointment.EndTime - appointment.StartTime).TotalMinutes
        };
    }

    /// <summary>
    /// Obtiene IDs de todos los providers activos del business
    /// </summary>
    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        return await _providerService.GetProviderIdsByBusinessAsync(businessId);
    }
}

