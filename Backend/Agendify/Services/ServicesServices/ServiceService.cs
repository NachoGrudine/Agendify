﻿using Agendify.Models.Entities;
using Agendify.DTOs.Service;
using Agendify.Repositories;
using Agendify.Common.Errors;
using FluentResults;

namespace Agendify.Services.ServicesServices;

public class ServiceService : IServiceService
{
    private readonly IRepository<Service> _serviceRepository;

    public ServiceService(IRepository<Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<Result<ServiceResponseDto>> CreateAsync(int businessId, CreateServiceDto dto)
    {
        var service = new Service
        {
            BusinessId = businessId,
            Name = dto.Name,
            DefaultDuration = dto.DefaultDuration,
            Price = dto.Price
        };

        var createdService = await _serviceRepository.AddAsync(service);
        return Result.Ok(MapToResponseDto(createdService));
    }

    public async Task<Result<ServiceResponseDto>> UpdateAsync(int businessId, int id, UpdateServiceDto dto)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Servicio no encontrado"));
        }

        service.Name = dto.Name;
        service.DefaultDuration = dto.DefaultDuration;
        service.Price = dto.Price;

        var updated = await _serviceRepository.UpdateAsync(service);
        return Result.Ok(MapToResponseDto(updated));
    }

    public async Task<Result<ServiceResponseDto>> GetByIdAsync(int businessId, int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Servicio no encontrado"));
        }

        return Result.Ok(MapToResponseDto(service));
    }

    public async Task<IEnumerable<ServiceResponseDto>> GetByBusinessAsync(int businessId)
    {
        var services = await _serviceRepository.FindAsync(s => s.BusinessId == businessId);
        return services.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<ServiceResponseDto>> SearchByNameAsync(int businessId, string name)
    {
        var services = await _serviceRepository.FindAsync(s =>
            s.BusinessId == businessId &&
            s.Name.ToLower().Contains(name.ToLower()));
        return services.Select(MapToResponseDto);
    }

    public async Task<Result> DeleteAsync(int businessId, int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Servicio no encontrado"));
        }

        service.IsDeleted = true;
        await _serviceRepository.UpdateAsync(service);
        
        return Result.Ok();
    }

    public async Task<int?> ResolveOrCreateAsync(int businessId, int? serviceId, string? serviceName, int defaultDuration)
    {
        // Si hay un ID, validar que exista y pertenezca al business
        if (serviceId.HasValue)
        {
            var existing = await _serviceRepository.GetByIdAsync(serviceId.Value);
            if (existing != null && existing.BusinessId == businessId && !existing.IsDeleted)
            {
                return serviceId.Value;
            }
            // Si el ID no es válido, se ignora y se intenta crear por nombre
        }

        // Si hay nombre pero no ID válido, crear nuevo Service
        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            var newService = new Service
            {
                BusinessId = businessId,
                Name = serviceName.Trim(),
                DefaultDuration = defaultDuration,
                Price = null
            };

            var createdService = await _serviceRepository.AddAsync(newService);
            return createdService.Id;
        }

        // No hay ni ID válido ni nombre, devolver null
        return null;
    }

    private static ServiceResponseDto MapToResponseDto(Service service)
    {
        return new ServiceResponseDto
        {
            Id = service.Id,
            BusinessId = service.BusinessId,
            Name = service.Name,
            DefaultDuration = service.DefaultDuration,
            Price = service.Price
        };
    }
}

