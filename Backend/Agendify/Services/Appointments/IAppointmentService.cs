using Agendify.DTOs.Appointment;
using Agendify.DTOs.Common;
using Agendify.Models.Entities;
using FluentResults;

namespace Agendify.Services.Appointments;

public interface IAppointmentService
{
    Task<Result<AppointmentResponseDto>> CreateAsync(int businessId, CreateAppointmentDto dto);
    Task<Result<AppointmentResponseDto>> UpdateAsync(int businessId, int id, UpdateAppointmentDto dto);
    Task<Result<AppointmentResponseDto>> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<AppointmentResponseDto>> GetByBusinessAsync(int businessId);
    Task<IEnumerable<AppointmentResponseDto>> GetByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Appointment>> GetAppointmentsWithDetailsByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<PagedResultDto<AppointmentResponseDto>> GetPagedByDateAsync(int businessId, DateTime date, int page, int pageSize);
    Task<Result> DeleteAsync(int businessId, int id);
    
    /// <summary>
    /// Obtiene la tendencia de appointments comparando un día específico con el día anterior
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="date">Fecha a consultar</param>
    /// <returns>Diferencia de turnos: positivo si hay más que ayer, negativo si hay menos</returns>
    Task<int> GetAppointmentsTrendAsync(int businessId, DateTime date);
}

