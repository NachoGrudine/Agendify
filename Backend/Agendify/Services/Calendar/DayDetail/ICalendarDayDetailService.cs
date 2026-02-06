using Agendify.DTOs.Calendar;

namespace Agendify.Services.Calendar.DayDetail;

/// <summary>
/// Servicio especializado para obtener detalles completos de un día específico
/// </summary>
public interface ICalendarDayDetailService
{
    /// <summary>
    /// Obtiene detalles completos de un día específico con appointments y métricas de disponibilidad
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="date">Fecha del día a consultar</param>
    /// <param name="page">Número de página (paginación)</param>
    /// <param name="pageSize">Cantidad de elementos por página</param>
    /// <param name="startTimeFrom">Filtro por hora de inicio desde (formato HH:mm)</param>
    /// <param name="startTimeTo">Filtro por hora de inicio hasta (formato HH:mm)</param>
    /// <param name="searchText">Texto de búsqueda en customer, service o provider</param>
    /// <returns>Detalle completo del día con appointments paginados y métricas</returns>
    Task<DayDetailsDto> GetDayDetailsAsync(
        int businessId,
        DateTime date,
        int page = 1,
        int pageSize = 10,
        string? startTimeFrom = null,
        string? startTimeTo = null,
        string? searchText = null);
}

