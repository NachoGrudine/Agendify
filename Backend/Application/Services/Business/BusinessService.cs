﻿using BusinessEntity = Agendify.API.Domain.Entities.Business;
using Agendify.API.DTOs.Business;
using Agendify.API.Infrastructure.Repositories;

namespace Agendify.API.Services.Business;

public class BusinessService : IBusinessService
{
    private readonly IRepository<BusinessEntity> _businessRepository;

    public BusinessService(IRepository<BusinessEntity> businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<BusinessResponseDto?> GetByIdAsync(int id)
    {
        var business = await _businessRepository.GetByIdAsync(id);
        return business == null ? null : MapToResponseDto(business);
    }

    public async Task<BusinessResponseDto> UpdateAsync(int id, UpdateBusinessDto dto)
    {
        var business = await _businessRepository.GetByIdAsync(id);
        if (business == null)
        {
            throw new KeyNotFoundException("Negocio no encontrado");
        }

        business.Name = dto.Name;
        business.Industry = dto.Industry;

        var updated = await _businessRepository.UpdateAsync(business);
        return MapToResponseDto(updated);
    }

    private static BusinessResponseDto MapToResponseDto(BusinessEntity business)
    {
        return new BusinessResponseDto
        {
            Id = business.Id,
            Name = business.Name,
            Industry = business.Industry
        };
    }
}

