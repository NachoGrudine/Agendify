using Agendify.Common.Errors;
using Agendify.Models.Entities;
using Agendify.Models.Enums;
using Agendify.DTOs.Appointment;
using Agendify.DTOs.Common;
using Agendify.Repositories;
using Agendify.Services.Customers;
using Agendify.Services.ServicesServices;
using FluentResults;

namespace Agendify.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerService _customerService;
    private readonly IServiceService _serviceService;

    public AppointmentService(IAppointmentRepository appointmentRepository, ICustomerService customerService, IServiceService serviceService)
    {
        _appointmentRepository = appointmentRepository;
        _customerService = customerService;
        _serviceService = serviceService;
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

        // Resolver o crear Customer usando el servicio correspondiente
        int? customerId = await _customerService.ResolveOrCreateAsync(
            businessId, dto.CustomerId, dto.CustomerName);

        // Resolver o crear Service usando el servicio correspondiente
        var defaultDuration = (int)(dto.EndTime - dto.StartTime).TotalMinutes;
        int? serviceId = await _serviceService.ResolveOrCreateAsync(
            businessId, dto.ServiceId, dto.ServiceName, defaultDuration);

        var appointment = new Appointment
        {
            BusinessId = businessId,
            ProviderId = dto.ProviderId,
            CustomerId = customerId,
            ServiceId = serviceId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = AppointmentStatus.Pending,
            Notes = dto.Notes
        };

        var created = await _appointmentRepository.AddAsync(appointment);
        
        return Result.Ok(MapToResponseDto(created));
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

        // Resolver o crear Customer usando el servicio correspondiente
        int? customerId = await _customerService.ResolveOrCreateAsync(businessId, dto.CustomerId, dto.CustomerName);

        // Resolver o crear Service usando el servicio correspondiente
        var defaultDuration = (int)(dto.EndTime - dto.StartTime).TotalMinutes;
        
        int? serviceId = await _serviceService.ResolveOrCreateAsync(businessId, dto.ServiceId, dto.ServiceName, defaultDuration);

        appointment.ProviderId = dto.ProviderId;
        appointment.CustomerId = customerId;
        appointment.ServiceId = serviceId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Status = dto.Status;
        appointment.Notes = dto.Notes;

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
    
    public async Task<IEnumerable<Appointment>> GetAppointmentsWithDetailsByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate)
    {
        return await _appointmentRepository.GetByDateRangeAsync(businessId, startDate, endDate);
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

    /// <summary>
    /// Obtiene la tendencia de appointments comparando un día específico con el día anterior
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="date">Fecha a consultar</param>
    /// <returns>Diferencia de turnos: +N si hay más que ayer, -N si hay menos, 0 si es igual</returns>
    public async Task<int> GetAppointmentsTrendAsync(int businessId, DateTime date)
    {
        date = date.Date; // Normalizar a medianoche
        var previousDate = date.AddDays(-1);

        // Contar turnos del día consultado
        var todayAppointments = await _appointmentRepository.GetByDateRangeAsync(
            businessId, date, date.AddDays(1).AddSeconds(-1));
        var todayCount = todayAppointments.Count();

        // Contar turnos del día anterior
        var yesterdayAppointments = await _appointmentRepository.GetByDateRangeAsync(
            businessId, previousDate, previousDate.AddDays(1).AddSeconds(-1));
        var yesterdayCount = yesterdayAppointments.Count();

        // Retornar la diferencia
        return todayCount - yesterdayCount;
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
            CustomerName = appointment.Customer?.Name,
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service?.Name,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            Notes = appointment.Notes
        };
    }
}
