﻿using Agendify.API.DTOs.Business;

namespace Agendify.API.Services.Business;

public interface IBusinessService
{
    Task<BusinessResponseDto?> GetByIdAsync(int id);
    Task<BusinessResponseDto> UpdateAsync(int id, UpdateBusinessDto dto);
}

