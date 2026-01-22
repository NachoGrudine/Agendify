using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Appointment;
using Agendify.API.DTOs.Common;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<AppointmentResponseDto> CreateAsync(int businessId, CreateAppointmentDto dto)
    {
        // Validar que no haya conflictos de horarios
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            dto.ProviderId, dto.StartTime, dto.EndTime);

        if (hasConflict)
        {
            throw new InvalidOperationException("El proveedor ya tiene un turno asignado en ese horario");
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
        
        return MapToResponseDto(created!);
    }

    public async Task<AppointmentResponseDto> UpdateAsync(int businessId, int id, UpdateAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Turno no encontrado");
        }

        // Validar que no haya conflictos de horarios (excluyendo este turno)
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            dto.ProviderId, dto.StartTime, dto.EndTime, id);

        if (hasConflict)
        {
            throw new InvalidOperationException("El proveedor ya tiene un turno asignado en ese horario");
        }

        appointment.ProviderId = dto.ProviderId;
        appointment.CustomerId = dto.CustomerId;
        appointment.ServiceId = dto.ServiceId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Status = dto.Status;

        var updated = await _appointmentRepository.UpdateAsync(appointment);
        return MapToResponseDto(updated);
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int businessId, int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            return null;
        }

        return MapToResponseDto(appointment);
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

    public async Task DeleteAsync(int businessId, int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null || appointment.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Turno no encontrado");
        }

        appointment.IsDeleted = true;
        await _appointmentRepository.UpdateAsync(appointment);
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

