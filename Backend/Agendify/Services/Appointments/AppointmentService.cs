using Agendify.Common.Errors;
using Agendify.Models.Entities;
using Agendify.Models.Enums;
using Agendify.DTOs.Appointment;
using Agendify.DTOs.Common;
using Agendify.Repositories;
using Agendify.Data;
using FluentResults;

namespace Agendify.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AgendifyDbContext _context;

    public AppointmentService(IAppointmentRepository appointmentRepository, AgendifyDbContext context)
    {
        _appointmentRepository = appointmentRepository;
        _context = context;
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

        // Resolver o crear Customer
        int? customerId = await ResolveOrCreateCustomerAsync(businessId, dto.CustomerId, dto.CustomerName);

        // Resolver o crear Service
        int? serviceId = await ResolveOrCreateServiceAsync(businessId, dto.ServiceId, dto.ServiceName, dto.StartTime, dto.EndTime);

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

        // Resolver o crear Customer
        int? customerId = await ResolveOrCreateCustomerAsync(businessId, dto.CustomerId, dto.CustomerName);

        // Resolver o crear Service
        int? serviceId = await ResolveOrCreateServiceAsync(businessId, dto.ServiceId, dto.ServiceName, dto.StartTime, dto.EndTime);

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

    /// <summary>
    /// Resuelve o crea un Customer basado en CustomerId o CustomerName
    /// </summary>
    private async Task<int?> ResolveOrCreateCustomerAsync(int businessId, int? customerId, string? customerName)
    {
        // Si hay un ID, usarlo directamente
        if (customerId.HasValue)
        {
            return customerId.Value;
        }

        // Si hay nombre pero no ID, crear nuevo Customer
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var newCustomer = new Customer
            {
                BusinessId = businessId,
                Name = customerName.Trim(),
                Phone = null,
                Email = null
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            return newCustomer.Id;
        }

        // No hay ni ID ni nombre, devolver null
        return null;
    }

    /// <summary>
    /// Resuelve o crea un Service basado en ServiceId o ServiceName
    /// </summary>
    private async Task<int?> ResolveOrCreateServiceAsync(int businessId, int? serviceId, string? serviceName, DateTime startTime, DateTime endTime)
    {
        // Si hay un ID, usarlo directamente
        if (serviceId.HasValue)
        {
            return serviceId.Value;
        }

        // Si hay nombre pero no ID, crear nuevo Service
        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            // Calcular duración en minutos desde el rango de tiempo
            var defaultDuration = (int)(endTime - startTime).TotalMinutes;

            var newService = new Service
            {
                BusinessId = businessId,
                Name = serviceName.Trim(),
                DefaultDuration = defaultDuration,
                Price = null
            };

            _context.Services.Add(newService);
            await _context.SaveChangesAsync();

            return newService.Id;
        }

        // No hay ni ID ni nombre, devolver null
        return null;
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
            Status = appointment.Status,
            Notes = appointment.Notes
        };
    }
}

