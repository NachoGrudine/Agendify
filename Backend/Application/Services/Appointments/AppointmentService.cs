using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Appointment;
using Agendify.API.DTOs.Common;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRepository<Provider> _providerRepository;
    private readonly IRepository<ProviderSchedule> _scheduleRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IRepository<Provider> providerRepository,
        IRepository<ProviderSchedule> scheduleRepository)
    {
        _appointmentRepository = appointmentRepository;
        _providerRepository = providerRepository;
        _scheduleRepository = scheduleRepository;
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

    public async Task<IEnumerable<CalendarDaySummaryDto>> GetCalendarSummaryAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        var providerIds = await GetProviderIdsAsync(businessId);
        
        if (!providerIds.Any())
        {
            return GenerateEmptyDays(startDate, endDate);
        }

        var minutesByDayOfWeek = await CalculateScheduledMinutesByDayOfWeekAsync(providerIds);
        var appointmentsByDate = await CalculateOccupiedMinutesByDateAsync(businessId, startDate, endDate);

        return GenerateDailySummaries(startDate, endDate, minutesByDayOfWeek, appointmentsByDate);
    }

    private async Task<List<int>> GetProviderIdsAsync(int businessId)
    {
        var allProviders = await _providerRepository.FindAsync(p => p.BusinessId == businessId);
        return allProviders.Select(p => p.Id).ToList();
    }

    private async Task<Dictionary<DayOfWeek, int>> CalculateScheduledMinutesByDayOfWeekAsync(List<int> providerIds)
    {
        var allSchedules = await _scheduleRepository.FindAsync(s => providerIds.Contains(s.ProviderId));
        var minutesByDayOfWeek = new Dictionary<DayOfWeek, int>();

        foreach (var schedule in allSchedules)
        {
            var minutes = (int)(schedule.EndTime - schedule.StartTime).TotalMinutes;

            if (!minutesByDayOfWeek.ContainsKey(schedule.DayOfWeek))
            {
                minutesByDayOfWeek[schedule.DayOfWeek] = 0;
            }

            minutesByDayOfWeek[schedule.DayOfWeek] += minutes;
        }

        return minutesByDayOfWeek;
    }

    private async Task<Dictionary<DateTime, (int Count, int Minutes)>> CalculateOccupiedMinutesByDateAsync(
        int businessId, DateTime startDate, DateTime endDate)
    {
        var appointments = await _appointmentRepository.GetByDateRangeAsync(
            businessId, startDate, endDate.AddDays(1).AddSeconds(-1));

        return appointments
            .GroupBy(a => a.StartTime.Date)
            .ToDictionary(
                g => g.Key,
                g => (
                    Count: g.Count(),
                    Minutes: g.Sum(a => (int)(a.EndTime - a.StartTime).TotalMinutes)
                )
            );
    }

    private static List<CalendarDaySummaryDto> GenerateDailySummaries(
        DateTime startDate,
        DateTime endDate,
        Dictionary<DayOfWeek, int> minutesByDayOfWeek,
        Dictionary<DateTime, (int Count, int Minutes)> appointmentsByDate)
    {
        var summaries = new List<CalendarDaySummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var totalScheduledMinutes = minutesByDayOfWeek.GetValueOrDefault(dayOfWeek, 0);
            var (appointmentsCount, totalOccupiedMinutes) = appointmentsByDate.GetValueOrDefault(currentDate, (0, 0));
            var totalAvailableMinutes = Math.Max(0, totalScheduledMinutes - totalOccupiedMinutes);

            summaries.Add(new CalendarDaySummaryDto
            {
                Date = currentDate,
                AppointmentsCount = appointmentsCount,
                TotalScheduledMinutes = totalScheduledMinutes,
                TotalOccupiedMinutes = totalOccupiedMinutes,
                TotalAvailableMinutes = totalAvailableMinutes
            });

            currentDate = currentDate.AddDays(1);
        }

        return summaries;
    }

    private static IEnumerable<CalendarDaySummaryDto> GenerateEmptyDays(DateTime startDate, DateTime endDate)
    {
        var summaries = new List<CalendarDaySummaryDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            summaries.Add(new CalendarDaySummaryDto
            {
                Date = currentDate,
                AppointmentsCount = 0,
                TotalScheduledMinutes = 0,
                TotalOccupiedMinutes = 0,
                TotalAvailableMinutes = 0
            });

            currentDate = currentDate.AddDays(1);
        }

        return summaries;
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

