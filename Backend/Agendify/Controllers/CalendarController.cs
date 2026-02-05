using Agendify.DTOs.Calendar;
using Agendify.Services.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Calendar views controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Endpoints for calendar visualization with summaries and appointment details by date")]
public class CalendarController : BaseController
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }
    
    /// <summary>
    /// Gets calendar summary by date range
    /// </summary>
    /// <remarks>
    /// Returns appointment count, busy time and available time for each day in the range.
    /// Ideal for monthly/weekly view.
    /// </remarks>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Summary by day</returns>
    /// <response code="200">Summary retrieved</response>
    [HttpGet("summary")]
    [SwaggerOperation(
        Summary = "Calendar summary by date range",
        Description = "Gets daily summary with appointment count and times (for monthly/weekly view)",
        OperationId = "GetCalendarSummary",
        Tags = new[] { "Calendar" }
    )]
    public async Task<ActionResult<IEnumerable<CalendarDaySummaryDto>>> GetCalendarSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var businessId = GetBusinessId();
        var summary = await _calendarService.GetCalendarSummaryAsync(businessId, startDate, endDate);
        return Ok(summary);
    }

    /// <summary>
    /// Gets appointment details for a specific day
    /// </summary>
    /// <remarks>
    /// Returns complete appointment list for the day with pagination.
    /// Optional filters: startTime (start hour), searchText (searches in customer, service, provider).
    /// </remarks>
    /// <param name="date">Date of the day</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 15)</param>
    /// <param name="startTime">Filter from hour (HH:mm)</param>
    /// <param name="searchText">Search text</param>
    /// <returns>Day details with appointment list</returns>
    /// <response code="200">Details retrieved</response>
    [HttpGet("day/{date}")]
    [SwaggerOperation(
        Summary = "Appointment details for a specific day",
        Description = "Gets complete appointment list for a day with pagination and optional filters",
        OperationId = "GetDayDetails",
        Tags = new[] { "Calendar" }
    )]
    public async Task<ActionResult<DayDetailsDto>> GetDayDetails(
        DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? startTime = null,
        [FromQuery] string? searchText = null)
    {
        var businessId = GetBusinessId();
        var details = await _calendarService.GetDayDetailsAsync(
            businessId, 
            date,
            page,
            pageSize,
            startTime, 
            searchText);
        return Ok(details);
    }
}

