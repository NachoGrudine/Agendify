﻿using Agendify.DTOs.Business;
using Agendify.Services.Business;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Business information management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Business information and configuration (name, industry, etc.)")]
public class BusinessController : BaseController
{
    private readonly IBusinessService _businessService;
    

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }


    /// <summary>
    /// Gets authenticated business information
    /// </summary>
    /// <returns>Business data</returns>
    /// <response code="200">Information retrieved</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get business information",
        Description = "Returns complete information of the authenticated business",
        OperationId = "GetBusiness",
        Tags = new[] { "Business" }
    )]
    public async Task<ActionResult<BusinessResponseDto>> Get()
    {
        var businessId = GetBusinessId();
        var result = await _businessService.GetByIdAsync(businessId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates business information
    /// </summary>
    /// <param name="dto">New data (name, industry)</param>
    /// <returns>Updated business</returns>
    /// <response code="200">Successfully updated</response>
    [HttpPut]
    [SwaggerOperation(
        Summary = "Update business information",
        Description = "Modifies business name and industry",
        OperationId = "UpdateBusiness",
        Tags = new[] { "Business" }
    )]
    public async Task<ActionResult<BusinessResponseDto>> Update([FromBody] UpdateBusinessDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _businessService.UpdateAsync(businessId, dto);
        return result.ToActionResult();
    }
}
