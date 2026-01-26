﻿﻿using Agendify.DTOs.Appointment;
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
}

