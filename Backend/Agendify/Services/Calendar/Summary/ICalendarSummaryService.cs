using Agendify.DTOs.Calendar;

namespace Agendify.Services.Calendar.Summary;

/// <summary>
/// Servicio especializado para obtener resúmenes de calendario por rango de fechas
/// </summary>
public interface ICalendarSummaryService
{
    /// <summary>
    /// Obtiene resumen diario del calendario para un rango de fechas con minutos disponibles históricos
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="startDate">Fecha inicio del rango</param>
    /// <param name="endDate">Fecha fin del rango</param>
    /// <returns>Lista de summaries diarios con appointments count, minutos programados, ocupados y disponibles</returns>
    Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(
        int businessId, 
        DateTime startDate, 
        DateTime endDate);
}

