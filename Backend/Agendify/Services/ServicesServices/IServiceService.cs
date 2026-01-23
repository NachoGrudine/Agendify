using Agendify.DTOs.Service;

namespace Agendify.Services.ServicesServices;

public interface IServiceService
{
    Task<ServiceResponseDto> CreateAsync(int businessId, CreateServiceDto dto);
    Task<ServiceResponseDto> UpdateAsync(int businessId, int id, UpdateServiceDto dto);
    Task<ServiceResponseDto?> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<ServiceResponseDto>> GetByBusinessAsync(int businessId);
    Task DeleteAsync(int businessId, int id);
}
