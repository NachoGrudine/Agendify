﻿using Agendify.DTOs.ProviderSchedule;
using Agendify.Services.ProviderSchedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderSchedulesController : ControllerBase
{
    private readonly IProviderScheduleService _scheduleService;

    public ProviderSchedulesController(IProviderScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<IEnumerable<ProviderScheduleResponseDto>>> GetByProvider(int providerId)
    {
        var businessId = GetBusinessId();
        var schedules = await _scheduleService.GetByProviderAsync(businessId, providerId);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProviderScheduleResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var schedule = await _scheduleService.GetByIdAsync(businessId, id);

        if (schedule == null)
        {
            return NotFound();
        }

        return Ok(schedule);
    }

    [HttpPost]
    public async Task<ActionResult<ProviderScheduleResponseDto>> Create([FromBody] CreateProviderScheduleDto dto)
    {
        var businessId = GetBusinessId();
        var schedule = await _scheduleService.CreateAsync(businessId, dto);
        return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProviderScheduleResponseDto>> Update(int id, [FromBody] UpdateProviderScheduleDto dto)
    {
        var businessId = GetBusinessId();
        var schedule = await _scheduleService.UpdateAsync(businessId, id, dto);
        return Ok(schedule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        await _scheduleService.DeleteAsync(businessId, id);
        return NoContent();
    }
}

