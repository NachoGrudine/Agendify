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
    
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
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

