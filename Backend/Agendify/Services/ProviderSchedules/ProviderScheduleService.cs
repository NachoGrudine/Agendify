using Agendify.Models.Entities;
using Agendify.DTOs.ProviderSchedule;
using Agendify.Repositories;
using Agendify.Common.Errors;
using Agendify.Services.Providers;
using FluentResults;

namespace Agendify.Services.ProviderSchedules;

public class ProviderScheduleService : IProviderScheduleService
{
    private readonly IRepository<ProviderSchedule> _scheduleRepository;
    private readonly IProviderService _providerService;

    public ProviderScheduleService(
        IRepository<ProviderSchedule> scheduleRepository,
        IProviderService providerService)
    {
        _scheduleRepository = scheduleRepository;
        _providerService = providerService;
    }

    public async Task<Result<ProviderScheduleResponseDto>> CreateAsync(int businessId, CreateProviderScheduleDto dto)
    {
        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, dto.ProviderId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        var schedule = new ProviderSchedule
        {
            ProviderId = dto.ProviderId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        await _scheduleRepository.AddAsync(schedule);
        return Result.Ok(MapToResponseDto(schedule));
    }

    public async Task<Result<ProviderScheduleResponseDto>> UpdateAsync(int businessId, int id, UpdateProviderScheduleDto dto)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return Result.Fail(new NotFoundError("Horario no encontrado"));
        }

        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, schedule.ProviderId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;

        var updated = await _scheduleRepository.UpdateAsync(schedule);
        return Result.Ok(MapToResponseDto(updated));
    }

    public async Task<Result<ProviderScheduleResponseDto>> GetByIdAsync(int businessId, int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return Result.Fail(new NotFoundError("Horario no encontrado"));
        }

        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, schedule.ProviderId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        return Result.Ok(MapToResponseDto(schedule));
    }

    public async Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByProviderAsync(int businessId, int providerId)
    {
        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, providerId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        var schedules = await _scheduleRepository.FindAsync(s => s.ProviderId == providerId);
        return Result.Ok(schedules.Select(MapToResponseDto));
    }

    public async Task<Dictionary<DayOfWeek, int>> GetScheduledMinutesByProviderIdsAsync(List<int> providerIds)
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

    public async Task<Result> DeleteAsync(int businessId, int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return Result.Fail(new NotFoundError("Horario no encontrado"));
        }

        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, schedule.ProviderId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        schedule.IsDeleted = true;
        await _scheduleRepository.UpdateAsync(schedule);
        
        return Result.Ok();
    }

    public async Task<Result> CreateDefaultSchedulesAsync(int providerId)
    {
        // Crear horarios por defecto de lunes a viernes de 09:00 a 18:00
        var defaultSchedules = new List<ProviderSchedule>();
        
        for (int day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
        {
            var schedule = new ProviderSchedule
            {
                ProviderId = providerId,
                DayOfWeek = (DayOfWeek)day,
                StartTime = new TimeSpan(9, 0, 0),  // 09:00
                EndTime = new TimeSpan(18, 0, 0)     // 18:00
            };
            
            defaultSchedules.Add(schedule);
        }

        // Agregar todos los horarios
        foreach (var schedule in defaultSchedules)
        {
            await _scheduleRepository.AddAsync(schedule);
        }

        return Result.Ok();
    }

    private static ProviderScheduleResponseDto MapToResponseDto(ProviderSchedule schedule)
    {
        return new ProviderScheduleResponseDto
        {
            Id = schedule.Id,
            ProviderId = schedule.ProviderId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime
        };
    }
}

