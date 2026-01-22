using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Service;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.BusinessServices;

public class ServiceService : IServiceService
{
    private readonly IRepository<Service> _serviceRepository;

    public ServiceService(IRepository<Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceResponseDto> CreateAsync(int businessId, CreateServiceDto dto)
    {
        var service = new Service
        {
            BusinessId = businessId,
            Name = dto.Name,
            DefaultDuration = dto.DefaultDuration,
            Price = dto.Price
        };

        await _serviceRepository.AddAsync(service);
        return MapToResponseDto(service);
    }

    public async Task<ServiceResponseDto> UpdateAsync(int businessId, int id, UpdateServiceDto dto)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Servicio no encontrado");
        }

        service.Name = dto.Name;
        service.DefaultDuration = dto.DefaultDuration;
        service.Price = dto.Price;

        var updated = await _serviceRepository.UpdateAsync(service);
        return MapToResponseDto(updated);
    }

    public async Task<ServiceResponseDto?> GetByIdAsync(int businessId, int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            return null;
        }

        return MapToResponseDto(service);
    }

    public async Task<IEnumerable<ServiceResponseDto>> GetByBusinessAsync(int businessId)
    {
        var services = await _serviceRepository.FindAsync(s => s.BusinessId == businessId);
        return services.Select(MapToResponseDto);
    }

    public async Task DeleteAsync(int businessId, int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null || service.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Servicio no encontrado");
        }

        service.IsDeleted = true;
        await _serviceRepository.UpdateAsync(service);
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

