using Agendify.API.Domain.Entities;
using Agendify.API.DTOs.Provider;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.Providers;

public class ProviderService : IProviderService
{
    private readonly IRepository<Provider> _providerRepository;

    public ProviderService(IRepository<Provider> providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<ProviderResponseDto> CreateAsync(int businessId, CreateProviderDto dto)
    {
        var provider = new Provider
        {
            BusinessId = businessId,
            Name = dto.Name,
            Specialty = dto.Specialty,
            IsActive = dto.IsActive
        };

        await _providerRepository.AddAsync(provider);
        return MapToResponseDto(provider);
    }

    public async Task<ProviderResponseDto> UpdateAsync(int businessId, int id, UpdateProviderDto dto)
    {
        var provider = await _providerRepository.GetByIdAsync(id);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Proveedor no encontrado");
        }

        provider.Name = dto.Name;
        provider.Specialty = dto.Specialty;
        provider.IsActive = dto.IsActive;

        var updated = await _providerRepository.UpdateAsync(provider);
        return MapToResponseDto(updated);
    }

    public async Task<ProviderResponseDto?> GetByIdAsync(int businessId, int id)
    {
        var provider = await _providerRepository.GetByIdAsync(id);
        if (provider == null || provider.BusinessId != businessId)
        {
            return null;
        }

        return MapToResponseDto(provider);
    }

    public async Task<IEnumerable<ProviderResponseDto>> GetByBusinessAsync(int businessId)
    {
        var providers = await _providerRepository.FindAsync(p => p.BusinessId == businessId);
        return providers.Select(MapToResponseDto);
    }

    public async Task DeleteAsync(int businessId, int id)
    {
        var provider = await _providerRepository.GetByIdAsync(id);
        if (provider == null || provider.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Proveedor no encontrado");
        }

        provider.IsDeleted = true;
        await _providerRepository.UpdateAsync(provider);
    }

    private static ProviderResponseDto MapToResponseDto(Provider provider)
    {
        return new ProviderResponseDto
        {
            Id = provider.Id,
            BusinessId = provider.BusinessId,
            Name = provider.Name,
            Specialty = provider.Specialty,
            IsActive = provider.IsActive
        };
    }
}

