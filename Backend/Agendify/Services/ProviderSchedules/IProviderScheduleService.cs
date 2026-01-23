using Agendify.DTOs.ProviderSchedule;

namespace Agendify.Services.ProviderSchedules;

public interface IProviderScheduleService
{
    Task<ProviderScheduleResponseDto> CreateAsync(int businessId, CreateProviderScheduleDto dto);
    Task<ProviderScheduleResponseDto> UpdateAsync(int businessId, int id, UpdateProviderScheduleDto dto);
    Task<ProviderScheduleResponseDto?> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<ProviderScheduleResponseDto>> GetByProviderAsync(int businessId, int providerId);
    Task DeleteAsync(int businessId, int id);
}
