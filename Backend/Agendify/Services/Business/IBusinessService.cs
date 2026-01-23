using Agendify.DTOs.Business;

namespace Agendify.Services.Business;

public interface IBusinessService
{
    Task<BusinessResponseDto?> GetByIdAsync(int id);
    Task<BusinessResponseDto> UpdateAsync(int id, UpdateBusinessDto dto);
}

