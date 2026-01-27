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


    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetByProvider(int providerId)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return result.ToActionResult();
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ProviderScheduleResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ProviderScheduleResponseDto>> Create([FromBody] CreateProviderScheduleDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProviderScheduleResponseDto>> Update(int id, [FromBody] UpdateProviderScheduleDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.UpdateAsync(businessId, id, dto);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _scheduleService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

