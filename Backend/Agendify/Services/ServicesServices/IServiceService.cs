﻿using Agendify.DTOs.Service;
using FluentResults;

namespace Agendify.Services.ServicesServices;

public interface IServiceService
{
    Task<Result<ServiceResponseDto>> CreateAsync(int businessId, CreateServiceDto dto);
    Task<Result<ServiceResponseDto>> UpdateAsync(int businessId, int id, UpdateServiceDto dto);
    Task<Result<ServiceResponseDto>> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<ServiceResponseDto>> GetByBusinessAsync(int businessId);
    Task<IEnumerable<ServiceResponseDto>> SearchByNameAsync(int businessId, string name);
    Task<Result> DeleteAsync(int businessId, int id);
}
