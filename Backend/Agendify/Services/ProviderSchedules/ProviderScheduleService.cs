﻿using Agendify.Models.Entities;
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

    public async Task<Result<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateAsync(
        int businessId, 
        int providerId, 
        BulkUpdateProviderSchedulesDto dto)
    {
        // Verificar que el provider pertenece al business
        var providerResult = await _providerService.GetByIdAsync(businessId, providerId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        // Validación adicional: StartTime debe ser menor a EndTime
        foreach (var item in dto.Schedules)
        {
            if (item.StartTime >= item.EndTime)
            {
                return Result.Fail(new BadRequestError(
                    $"La hora de inicio debe ser menor a la hora de fin para el día {item.DayOfWeek}"));
            }
        }

        // 1. Identificar días afectados
        var affectedDays = dto.Schedules.Select(s => s.DayOfWeek).Distinct().ToList();

        // 2. Limpieza específica (Wipe): Eliminar SOLO los horarios de los días afectados
        var existingSchedules = await _scheduleRepository.FindAsync(
            s => s.ProviderId == providerId && affectedDays.Contains(s.DayOfWeek));
        
        var schedulesList = existingSchedules.ToList();
        if (schedulesList.Any())
        {
            await _scheduleRepository.DeleteRangeAsync(schedulesList);
        }

        // 3. Inserción (Replace): Insertar los nuevos horarios
        var newSchedules = new List<ProviderSchedule>();
        foreach (var item in dto.Schedules)
        {
            var schedule = new ProviderSchedule
            {
                ProviderId = providerId,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            };
            
            var created = await _scheduleRepository.AddAsync(schedule);
            newSchedules.Add(created);
        }

        return Result.Ok(newSchedules.Select(MapToResponseDto));
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

