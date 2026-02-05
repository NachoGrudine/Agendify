﻿using Agendify.DTOs.ProviderSchedule;
using Agendify.Services.ProviderSchedules;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Provider work schedules management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Provider availability schedules (working days and hours)")]
public class ProviderSchedulesController : BaseController
{
    private readonly IProviderScheduleService _scheduleService;

    public ProviderSchedulesController(IProviderScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }
    
    /// <summary>
    /// Gets authenticated provider's schedules
    /// </summary>
    /// <returns>List of schedules by day of week</returns>
    /// <response code="200">Schedules retrieved</response>
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Get my schedules",
        Description = "Gets the work schedules of the authenticated provider",
        OperationId = "GetMySchedules",
        Tags = new[] { "ProviderSchedules" }
    )]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetMySchedules()
    {
        var providerId = GetProviderId();
        var businessId = GetBusinessId();
        
        var result = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets schedules for a specific provider
    /// </summary>
    /// <param name="providerId">Provider ID</param>
    /// <returns>List of schedules</returns>
    /// <response code="200">Schedules retrieved</response>
    /// <response code="404">Not found</response>
    [HttpGet("provider/{providerId}")]
    [SwaggerOperation(
        Summary = "Get provider schedules",
        Description = "Gets the work schedules of a specific provider",
        OperationId = "GetProviderSchedules",
        Tags = new[] { "ProviderSchedules" }
    )]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetByProvider(int providerId)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return result.ToActionResult();
    }
    
    /// <summary>
    /// Updates authenticated provider's schedules (bulk)
    /// </summary>
    /// <remarks>
    /// Updates all week schedules in a single operation.
    /// day_of_week: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
    /// </remarks>
    /// <param name="dto">Schedule list (schedules with day_of_week, start_time, end_time, is_available)</param>
    /// <returns>Updated schedules</returns>
    /// <response code="200">Successfully updated</response>
    [HttpPut("me/bulk-update")]
    [SwaggerOperation(
        Summary = "Update my schedules (bulk)",
        Description = "Updates all week schedules of the authenticated provider in a single operation",
        OperationId = "BulkUpdateMySchedules",
        Tags = new[] { "ProviderSchedules" }
    )]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateMySchedules(
        [FromBody] BulkUpdateProviderSchedulesDto dto)
    {
        var providerId = GetProviderId();
        var businessId = GetBusinessId();
        
        var result = await _scheduleService.BulkUpdateAsync(businessId, providerId, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates a provider's schedules (bulk)
    /// </summary>
    /// <param name="providerId">Provider ID</param>
    /// <param name="dto">Schedule list</param>
    /// <returns>Updated schedules</returns>
    /// <response code="200">Successfully updated</response>
    /// <response code="404">Not found</response>
    [HttpPut("provider/{providerId}/bulk-update")]
    [SwaggerOperation(
        Summary = "Update provider schedules (bulk)",
        Description = "Updates all schedules of a specific provider in a single operation",
        OperationId = "BulkUpdateProviderSchedules",
        Tags = new[] { "ProviderSchedules" }
    )]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdate(
        int providerId,
        [FromBody] BulkUpdateProviderSchedulesDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.BulkUpdateAsync(businessId, providerId, dto);
        return result.ToActionResult();
    }
}

