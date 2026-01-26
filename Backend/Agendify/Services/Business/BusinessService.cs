using BusinessEntity = Agendify.Models.Entities.Business;
using Agendify.DTOs.Business;
using Agendify.Repositories;
using Agendify.Common.Errors;
using FluentResults;

namespace Agendify.Services.Business;

public class BusinessService : IBusinessService
{
    private readonly IRepository<BusinessEntity> _businessRepository;

    public BusinessService(IRepository<BusinessEntity> businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<Result<BusinessResponseDto>> GetByIdAsync(int id)
    {
        var business = await _businessRepository.GetByIdAsync(id);
        if (business == null)
        {
            return Result.Fail(new NotFoundError("Negocio no encontrado"));
        }
        
        return Result.Ok(MapToResponseDto(business));
    }

    public async Task<Result<BusinessResponseDto>> UpdateAsync(int id, UpdateBusinessDto dto)
    {
        var business = await _businessRepository.GetByIdAsync(id);
        if (business == null)
        {
            return Result.Fail(new NotFoundError("Negocio no encontrado"));
        }

        business.Name = dto.Name;
        business.Industry = dto.Industry;

        var updated = await _businessRepository.UpdateAsync(business);
        return Result.Ok(MapToResponseDto(updated));
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

