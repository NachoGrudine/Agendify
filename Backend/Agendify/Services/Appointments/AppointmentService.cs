﻿using Agendify.Common.Errors;
using Agendify.Models.Entities;
using Agendify.Models.Enums;
using Agendify.DTOs.Appointment;
using Agendify.DTOs.Common;
using Agendify.Repositories;
using FluentResults;

namespace Agendify.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<AppointmentResponseDto>> CreateAsync(int businessId, CreateAppointmentDto dto)
    {
        // Validar que no haya conflictos de horarios
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            dto.ProviderId, dto.StartTime, dto.EndTime);

        if (hasConflict)
        {
            return Result.Fail(new ConflictError("El proveedor ya tiene un turno asignado en ese horario"));
        }

        var appointment = new Appointment
        {
            BusinessId = businessId,
            ProviderId = dto.ProviderId,
            CustomerId = dto.CustomerId,
            ServiceId = dto.ServiceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = AppointmentStatus.Pending
        };

        var created = await _appointmentRepository.AddAsync(appointment);
        
        return Result.Ok(MapToResponseDto(created!));
    }

    public async Task<Result<AppointmentResponseDto>> UpdateAsync(int businessId, int id, UpdateAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Turno no encontrado"));
        }

        // Validar que no haya conflictos de horarios (excluyendo este turno)
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            dto.ProviderId, dto.StartTime, dto.EndTime, id);

        if (hasConflict)
        {
            return Result.Fail(new ConflictError("El proveedor ya tiene un turno asignado en ese horario"));
        }

        appointment.ProviderId = dto.ProviderId;
        appointment.CustomerId = dto.CustomerId;
        appointment.ServiceId = dto.ServiceId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Status = dto.Status;

        var updated = await _appointmentRepository.UpdateAsync(appointment);
        return Result.Ok(MapToResponseDto(updated));
    }

    public async Task<Result<AppointmentResponseDto>> GetByIdAsync(int businessId, int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Turno no encontrado"));
        }

        return Result.Ok(MapToResponseDto(appointment));
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetByBusinessAsync(int businessId)
    {
        var appointments = await _appointmentRepository.GetByBusinessIdAsync(businessId);
        return appointments.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate)
    {
        var appointments = await _appointmentRepository.GetByDateRangeAsync(businessId, startDate, endDate);
        return appointments.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsWithDetailsByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate)
    {
        return await _appointmentRepository.GetByDateRangeAsync(businessId, startDate, endDate);
    }

    public async Task<PagedResultDto<AppointmentResponseDto>> GetPagedByDateAsync(
        int businessId, DateTime date, int page, int pageSize)
    {
        var (items, totalCount) = await _appointmentRepository.GetPagedByDateAsync(businessId, date, page, pageSize);

        return new PagedResultDto<AppointmentResponseDto>
        {
            Items = items.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result> DeleteAsync(int businessId, int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Turno no encontrado"));
        }

        appointment.IsDeleted = true;
        await _appointmentRepository.UpdateAsync(appointment);
        
        return Result.Ok();
    }

    private static AppointmentResponseDto MapToResponseDto(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            BusinessId = appointment.BusinessId,
            ProviderId = appointment.ProviderId,
            ProviderName = appointment.Provider?.Name ?? string.Empty,
            CustomerId = appointment.CustomerId,
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service?.Name,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status
        };
    }
}

