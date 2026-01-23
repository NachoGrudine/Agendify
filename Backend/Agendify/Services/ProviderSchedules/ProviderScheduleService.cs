using Agendify.Models.Entities;
using Agendify.DTOs.ProviderSchedule;
using Agendify.Repositories;
using Agendify.Common.Errors;
using FluentResults;

namespace Agendify.Services.ProviderSchedules;

public class ProviderScheduleService : IProviderScheduleService
{
    private readonly IRepository<ProviderSchedule> _scheduleRepository;
    private readonly IRepository<Provider> _providerRepository;

    public ProviderScheduleService(
        IRepository<ProviderSchedule> scheduleRepository,
        IRepository<Provider> providerRepository)
    {
        _scheduleRepository = scheduleRepository;
        _providerRepository = providerRepository;
    }

    public async Task<Result<ProviderScheduleResponseDto>> CreateAsync(int businessId, CreateProviderScheduleDto dto)
    {
        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(dto.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Proveedor no encontrado o no pertenece al negocio"));
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
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return Result.Fail(new ForbiddenError("No autorizado"));
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
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return Result.Fail(new ForbiddenError("No autorizado"));
        }

        return Result.Ok(MapToResponseDto(schedule));
    }

    public async Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByProviderAsync(int businessId, int providerId)
    {
        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(providerId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Proveedor no encontrado o no pertenece al negocio"));
        }

        var schedules = await _scheduleRepository.FindAsync(s => s.ProviderId == providerId);
        return Result.Ok(schedules.Select(MapToResponseDto));
    }

    public async Task<Result> DeleteAsync(int businessId, int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return Result.Fail(new NotFoundError("Horario no encontrado"));
        }

        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return Result.Fail(new ForbiddenError("No autorizado"));
        }

        schedule.IsDeleted = true;
        await _scheduleRepository.UpdateAsync(schedule);
        
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

