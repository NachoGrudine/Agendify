﻿using Agendify.DTOs.Appointment;
using Agendify.Services.Appointments;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Appointments and bookings management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Complete appointment management: create, query, modify and cancel bookings")]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }
    
    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Complete appointment data</returns>
    /// <response code="200">Appointment found</response>
    /// <response code="404">Not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get appointment by ID",
        Description = "Returns complete information for a specific appointment of the authenticated business",
        OperationId = "GetAppointmentById",
        Tags = new[] { "Appointments" }
    )]
    public async Task<ActionResult<AppointmentResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <remarks>
    /// Allows booking with existing customer (customer_id) or walk-in (customer_name).
    /// Validates provider schedule conflicts.
    /// </remarks>
    /// <param name="dto">Appointment data (provider_id, customer_id/customer_name, service_id/service_name, start_time, end_time, notes)</param>
    /// <returns>Created appointment</returns>
    /// <response code="201">Successfully created</response>
    /// <response code="400">Invalid data</response>
    /// <response code="404">Provider, customer or service not found</response>
    /// <response code="409">Schedule conflict - provider already has an appointment at that time</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new appointment",
        Description = "Books a new appointment. Can be with existing customer or walk-in.",
        OperationId = "CreateAppointment",
        Tags = new[] { "Appointments" }
    )]
    public async Task<ActionResult<AppointmentResponseDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    /// <summary>
    /// Updates an existing appointment
    /// </summary>
    /// <remarks>
    /// Allows modifying date, time, provider, customer, service or appointment notes.
    /// Validates that it doesn't create schedule conflicts.
    /// </remarks>
    /// <param name="id">Appointment ID to update</param>
    /// <param name="dto">New appointment data</param>
    /// <returns>Updated appointment</returns>
    /// <response code="200">Successfully updated</response>
    /// <response code="400">Invalid data</response>
    /// <response code="404">Appointment not found</response>
    /// <response code="409">Schedule conflict - provider already has an appointment at that time</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update appointment",
        Description = "Modifies existing appointment data (schedule, provider, customer, etc.)",
        OperationId = "UpdateAppointment",
        Tags = new[] { "Appointments" }
    )]
    public async Task<ActionResult<AppointmentResponseDto>> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets the next scheduled appointment
    /// </summary>
    /// <param name="currentDateTime">Current date and time from frontend</param>
    /// <returns>Next appointment data: customer name, start-end time, and day</returns>
    /// <response code="200">Next appointment found</response>
    /// <response code="404">No upcoming appointments found</response>
    [HttpGet("next")]
    [SwaggerOperation(
        Summary = "Get next appointment",
        Description = "Returns the next scheduled appointment after the specified date/time",
        OperationId = "GetNextAppointment",
        Tags = new[] { "Appointments" }
    )]
    public async Task<ActionResult<NextAppointmentResponseDto>> GetNext([FromQuery] DateTime currentDateTime)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.GetNextAppointmentAsync(businessId, currentDateTime);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cancels/deletes an appointment (soft delete)
    /// </summary>
    /// <param name="id">Appointment ID to delete</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="204">Successfully deleted</response>
    /// <response code="404">Appointment not found</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete appointment",
        Description = "Cancels and deletes an appointment from the system (soft delete)",
        OperationId = "DeleteAppointment",
        Tags = new[] { "Appointments" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _appointmentService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

