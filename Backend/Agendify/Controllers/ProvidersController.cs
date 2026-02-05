﻿using Agendify.DTOs.Provider;
using Agendify.Services.Providers;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Service providers management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Business providers/professionals management (doctors, barbers, stylists, etc.)")]
public class ProvidersController : BaseController
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    /// <summary>
    /// Gets all business providers
    /// </summary>
    /// <returns>List of providers</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "List all providers",
        Description = "Gets all providers of the authenticated business",
        OperationId = "GetAllProviders",
        Tags = new[] { "Providers" }
    )]
    public async Task<ActionResult<IEnumerable<ProviderResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var providers = await _providerService.GetByBusinessAsync(businessId);
        return Ok(providers);
    }

    /// <summary>
    /// Gets a provider by ID
    /// </summary>
    /// <param name="id">Provider ID</param>
    /// <returns>Provider data</returns>
    /// <response code="200">Provider found</response>
    /// <response code="404">Not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get provider by ID",
        Description = "Returns complete information for a specific provider",
        OperationId = "GetProviderById",
        Tags = new[] { "Providers" }
    )]
    public async Task<ActionResult<ProviderResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new provider
    /// </summary>
    /// <param name="dto">Provider data (name, specialty, is_active)</param>
    /// <returns>Created provider</returns>
    /// <response code="201">Successfully created</response>
    /// <response code="400">Invalid data</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new provider",
        Description = "Adds a new professional/provider to the business",
        OperationId = "CreateProvider",
        Tags = new[] { "Providers" }
    )]
    public async Task<ActionResult<ProviderResponseDto>> Create([FromBody] CreateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    /// <summary>
    /// Updates a provider
    /// </summary>
    /// <param name="id">Provider ID</param>
    /// <param name="dto">New data</param>
    /// <returns>Updated provider</returns>
    /// <response code="200">Successfully updated</response>
    /// <response code="404">Not found</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update provider",
        Description = "Modifies existing provider data",
        OperationId = "UpdateProvider",
        Tags = new[] { "Providers" }
    )]
    public async Task<ActionResult<ProviderResponseDto>> Update(int id, [FromBody] UpdateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a provider (soft delete)
    /// </summary>
    /// <param name="id">Provider ID</param>
    /// <response code="204">Successfully deleted</response>
    /// <response code="404">Not found</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete provider",
        Description = "Deletes a provider from the system (soft delete)",
        OperationId = "DeleteProvider",
        Tags = new[] { "Providers" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

