using Agendify.DTOs.Calendar;
using Agendify.Services.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : BaseController
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }
    
    

    /// <summary>
    /// Obtiene un resumen del calendario para mostrar en vista mensual/semanal.
    /// Retorna por cada día: cantidad de turnos, tiempo ocupado y tiempo disponible.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<IEnumerable<CalendarDaySummaryDto>>> GetCalendarSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var businessId = GetBusinessId();
        var summary = await _calendarService.GetCalendarSummaryAsync(businessId, startDate, endDate);
        return Ok(summary);
    }

    /// <summary>
    /// Obtiene el detalle completo de un día específico con todos los turnos.
    /// Retorna: información del día, lista de turnos con cliente, proveedor, horario y duración.
    /// Se puede filtrar por estado, hora, o texto de búsqueda general (busca en cliente, servicio y proveedor).
    /// </summary>
    [HttpGet("day/{date}")]
    public async Task<ActionResult<DayDetailsDto>> GetDayDetails(
        DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? status = null,
        [FromQuery] string? startTime = null,
        [FromQuery] string? searchText = null)
    {
        var businessId = GetBusinessId();
        var details = await _calendarService.GetDayDetailsAsync(
            businessId, 
            date,
            page,
            pageSize,
            status,
            startTime, 
            searchText);
        return Ok(details);
    }
}

