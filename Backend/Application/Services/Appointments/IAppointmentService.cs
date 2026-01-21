﻿using Agendify.API.DTOs.Appointment;
using Agendify.API.DTOs.Common;

namespace Agendify.API.Services.Appointments;

public interface IAppointmentService
{
    Task<AppointmentResponseDto> CreateAsync(int businessId, CreateAppointmentDto dto);
    Task<AppointmentResponseDto> UpdateAsync(int businessId, int id, UpdateAppointmentDto dto);
    Task<AppointmentResponseDto?> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<AppointmentResponseDto>> GetByBusinessAsync(int businessId);
    Task<IEnumerable<AppointmentResponseDto>> GetByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<PagedResultDto<AppointmentResponseDto>> GetPagedByDateAsync(int businessId, DateTime date, int page, int pageSize);
    Task DeleteAsync(int businessId, int id);
 
}

