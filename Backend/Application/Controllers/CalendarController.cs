using Agendify.API.DTOs.Calendar;
using Agendify.API.Services.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
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
    /// Se puede filtrar por estado, hora, nombre de cliente o nombre de proveedor.
    /// </summary>
    [HttpGet("day/{date}")]
    public async Task<ActionResult<DayDetailsDto>> GetDayDetails(
        DateTime date,
        [FromQuery] string? status = null,
        [FromQuery] string? startTime = null,
        [FromQuery] string? customerName = null,
        [FromQuery] string? providerName = null)
    {
        var businessId = GetBusinessId();
        var details = await _calendarService.GetDayDetailsAsync(
            businessId, 
            date, 
            status, 
            startTime, 
            customerName, 
            providerName);
        return Ok(details);
    }
}

