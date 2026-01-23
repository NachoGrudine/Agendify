﻿﻿using Agendify.DTOs.Provider;
using FluentResults;

namespace Agendify.Services.Providers;

public interface IProviderService
{
    Task<Result<ProviderResponseDto>> CreateAsync(int businessId, CreateProviderDto dto);
    Task<Result<ProviderResponseDto>> UpdateAsync(int businessId, int id, UpdateProviderDto dto);
    Task<Result<ProviderResponseDto>> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<ProviderResponseDto>> GetByBusinessAsync(int businessId);
    Task<Result> DeleteAsync(int businessId, int id);
}
