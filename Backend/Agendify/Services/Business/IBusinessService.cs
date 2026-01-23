﻿using Agendify.DTOs.Business;
using FluentResults;

namespace Agendify.Services.Business;

public interface IBusinessService
{
    Task<Result<BusinessResponseDto>> GetByIdAsync(int id);
    Task<Result<BusinessResponseDto>> UpdateAsync(int id, UpdateBusinessDto dto);
}

