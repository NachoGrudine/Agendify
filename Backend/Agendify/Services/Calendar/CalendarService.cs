using Agendify.DTOs.Calendar;
using Agendify.Services.Calendar.DayDetail;
using Agendify.Services.Calendar.Summary;

namespace Agendify.Services.Calendar;

/// <summary>
/// Servicio orquestador de calendario - define el contrato público y delega a servicios especializados
/// Responsabilidad: coordinar y exponer la funcionalidad de calendario sin implementar lógica de negocio
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly ICalendarSummaryService _summaryService;
    private readonly ICalendarDayDetailService _dayDetailService;

    public CalendarService(
        ICalendarSummaryService summaryService,
        ICalendarDayDetailService dayDetailService)
    {
        _summaryService = summaryService;
        _dayDetailService = dayDetailService;
    }

    /// <summary>
    /// Obtiene resumen diario del calendario para un rango de fechas
    /// </summary>
    public async Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        return await _summaryService.GetCalendarSummaryAsync(businessId, startDate, endDate);
    }

    /// <summary>
    /// Obtiene detalles completos de un día específico con appointments paginados
    /// </summary>
    public async Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId,
        DateTime date,
        int page = 1,
        int pageSize = 10,
        string? startTime = null,
        string? searchText = null)
    {
        return await _dayDetailService.GetDayDetailsAsync(
            businessId, date, page, pageSize, startTime, searchText);
    }
}

