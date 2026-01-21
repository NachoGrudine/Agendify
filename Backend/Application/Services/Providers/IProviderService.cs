﻿using Agendify.API.DTOs.Provider;

namespace Agendify.API.Services.Providers;

public interface IProviderService
{
    Task<ProviderResponseDto> CreateAsync(int businessId, CreateProviderDto dto);
    Task<ProviderResponseDto> UpdateAsync(int businessId, int id, UpdateProviderDto dto);
    Task<ProviderResponseDto?> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<ProviderResponseDto>> GetByBusinessAsync(int businessId);
    Task DeleteAsync(int businessId, int id);
}
