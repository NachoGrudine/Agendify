﻿using Agendify.API.DTOs.Appointment;
using Agendify.API.DTOs.Common;
using Agendify.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Agendify.API.Services.Appointments;

namespace Agendify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var appointments = await _appointmentService.GetByBusinessAsync(businessId);
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var appointment = await _appointmentService.GetByIdAsync(businessId, id);

        if (appointment == null)
        {
            return NotFound();
        }

        return Ok(appointment);
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<PagedResultDto<AppointmentResponseDto>>> GetByDate(
        DateTime date, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 5)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.GetPagedByDateAsync(businessId, date, page, pageSize);
        return Ok(result);
    }


    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var businessId = GetBusinessId();
        var appointments = await _appointmentService.GetByDateRangeAsync(businessId, startDate, endDate);
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        var businessId = GetBusinessId();
        var appointment = await _appointmentService.CreateAsync(businessId, dto);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentResponseDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        var businessId = GetBusinessId();
        var appointment = await _appointmentService.UpdateAsync(businessId, id, dto);
        return Ok(appointment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        await _appointmentService.DeleteAsync(businessId, id);
        return NoContent();
    }
    
}

