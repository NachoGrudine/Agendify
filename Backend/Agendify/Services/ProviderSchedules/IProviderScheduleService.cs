using Agendify.DTOs.ProviderSchedule;
using FluentResults;

namespace Agendify.Services.ProviderSchedules;

public interface IProviderScheduleService
{
    Task<Result<ProviderScheduleResponseDto>> CreateAsync(int businessId, CreateProviderScheduleDto dto);
    Task<Result<ProviderScheduleResponseDto>> UpdateAsync(int businessId, int id, UpdateProviderScheduleDto dto);
    Task<Result<ProviderScheduleResponseDto>> GetByIdAsync(int businessId, int id);
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByProviderAsync(int businessId, int providerId);
    Task<Dictionary<DayOfWeek, int>> GetScheduledMinutesByProviderIdsAsync(List<int> providerIds);
    Task<Result> DeleteAsync(int businessId, int id);
    Task<Result> CreateDefaultSchedulesAsync(int providerId);
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateAsync(int businessId, int providerId, BulkUpdateProviderSchedulesDto dto);
    
    // Métodos basados en UserId
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByUserIdAsync(int userId);
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateByUserIdAsync(int userId, BulkUpdateProviderSchedulesDto dto);
}


