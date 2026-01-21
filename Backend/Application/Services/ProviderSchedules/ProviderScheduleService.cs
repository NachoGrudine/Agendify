using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.ProviderSchedule;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.ProviderSchedules;

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

    public async Task<ProviderScheduleResponseDto> CreateAsync(int businessId, CreateProviderScheduleDto dto)
    {
        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(dto.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new InvalidOperationException("Proveedor no encontrado o no pertenece al negocio");
        }

        var schedule = new ProviderSchedule
        {
            ProviderId = dto.ProviderId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        await _scheduleRepository.AddAsync(schedule);
        return MapToResponseDto(schedule);
    }

    public async Task<ProviderScheduleResponseDto> UpdateAsync(int businessId, int id, UpdateProviderScheduleDto dto)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Horario no encontrado");
        }

        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new InvalidOperationException("No autorizado");
        }

        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;

        var updated = await _scheduleRepository.UpdateAsync(schedule);
        return MapToResponseDto(updated);
    }

    public async Task<ProviderScheduleResponseDto?> GetByIdAsync(int businessId, int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            return null;
        }

        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            return null;
        }

        return MapToResponseDto(schedule);
    }

    public async Task<IEnumerable<ProviderScheduleResponseDto>> GetByProviderAsync(int businessId, int providerId)
    {
        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(providerId);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new InvalidOperationException("Proveedor no encontrado o no pertenece al negocio");
        }

        var schedules = await _scheduleRepository.FindAsync(s => s.ProviderId == providerId);
        return schedules.Select(MapToResponseDto);
    }

    public async Task DeleteAsync(int businessId, int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Horario no encontrado");
        }

        // Verificar que el provider pertenece al business
        var provider = await _providerRepository.GetByIdAsync(schedule.ProviderId);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new InvalidOperationException("No autorizado");
        }

        schedule.IsDeleted = true;
        await _scheduleRepository.UpdateAsync(schedule);
    }

    private static ProviderScheduleResponseDto MapToResponseDto(ProviderSchedule schedule)
    {
        return new ProviderScheduleResponseDto
        {
            Id = schedule.Id,
            ProviderId = schedule.ProviderId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }
}

