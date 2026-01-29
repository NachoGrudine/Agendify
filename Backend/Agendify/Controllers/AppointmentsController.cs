using Agendify.DTOs.Appointment;
using Agendify.DTOs.Common;
using Agendify.Services.Appointments;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
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
        var result = await _appointmentService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
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
        var result = await _appointmentService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentResponseDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

