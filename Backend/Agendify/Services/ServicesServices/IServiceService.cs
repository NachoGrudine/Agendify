﻿﻿using Agendify.DTOs.Service;
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
    
    /// <summary>
    /// Resuelve un service existente por ID o crea uno nuevo con el nombre y duración proporcionados
    /// </summary>
    /// <returns>ID del service resuelto o creado, o null si no se proporcionan datos</returns>
    Task<int?> ResolveOrCreateAsync(int businessId, int? serviceId, string? serviceName, int defaultDuration);
}
