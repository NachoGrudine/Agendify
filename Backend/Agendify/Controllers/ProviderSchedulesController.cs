using Agendify.DTOs.ProviderSchedule;
using Agendify.Services.ProviderSchedules;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderSchedulesController : BaseController
{
    private readonly IProviderScheduleService _scheduleService;

    public ProviderSchedulesController(IProviderScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetMySchedules()
    {
        var providerId = GetProviderId();
        var businessId = GetBusinessId();
        
        var result = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return result.ToActionResult();
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetByProvider(int providerId)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return result.ToActionResult();
    }
    
    [HttpPut("me/bulk-update")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateMySchedules(
        [FromBody] BulkUpdateProviderSchedulesDto dto)
    {
        var providerId = GetProviderId();
        var businessId = GetBusinessId();
        
        var result = await _scheduleService.BulkUpdateAsync(businessId, providerId, dto);
        return result.ToActionResult();
    }

    [HttpPut("provider/{providerId}/bulk-update")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdate(
        int providerId,
        [FromBody] BulkUpdateProviderSchedulesDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.BulkUpdateAsync(businessId, providerId, dto);
        return result.ToActionResult();
    }
}

