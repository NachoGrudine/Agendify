using Agendify.DTOs.Service;
using Agendify.Services.ServicesServices;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Services catalog management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Services catalog management (haircut, consultation, massage, etc.) with duration and price")]
public class ServicesController : BaseController
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    /// <summary>
    /// Gets all business services
    /// </summary>
    /// <returns>List of services</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "List all services",
        Description = "Gets the complete services catalog of the business",
        OperationId = "GetAllServices",
        Tags = new[] { "Services" }
    )]
    public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var services = await _serviceService.GetByBusinessAsync(businessId);
        return Ok(services);
    }

    /// <summary>
    /// Searches services by name (partial, case-insensitive)
    /// </summary>
    /// <param name="name">Text to search</param>
    /// <returns>List of matching services</returns>
    /// <response code="200">Search completed</response>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Search services by name",
        Description = "Partial search of services by name (useful for autocomplete)",
        OperationId = "SearchServices",
        Tags = new[] { "Services" }
    )]
    public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> Search([FromQuery] string name)
    {
        var businessId = GetBusinessId();
        var services = await _serviceService.SearchByNameAsync(businessId, name);
        return Ok(services);
    }

    /// <summary>
    /// Gets a service by ID
    /// </summary>
    /// <param name="id">Service ID</param>
    /// <returns>Service data</returns>
    /// <response code="200">Service found</response>
    /// <response code="404">Not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get service by ID",
        Description = "Returns complete information for a specific service",
        OperationId = "GetServiceById",
        Tags = new[] { "Services" }
    )]
    public async Task<ActionResult<ServiceResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new service
    /// </summary>
    /// <param name="dto">Service data (name, default_duration in minutes, optional price)</param>
    /// <returns>Created service</returns>
    /// <response code="201">Successfully created</response>
    /// <response code="400">Invalid data</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new service",
        Description = "Adds a new service to the business catalog",
        OperationId = "CreateService",
        Tags = new[] { "Services" }
    )]
    public async Task<ActionResult<ServiceResponseDto>> Create([FromBody] CreateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    /// <summary>
    /// Updates a service
    /// </summary>
    /// <param name="id">Service ID</param>
    /// <param name="dto">New data</param>
    /// <returns>Updated service</returns>
    /// <response code="200">Successfully updated</response>
    /// <response code="404">Not found</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update service",
        Description = "Modifies existing service data",
        OperationId = "UpdateService",
        Tags = new[] { "Services" }
    )]
    public async Task<ActionResult<ServiceResponseDto>> Update(int id, [FromBody] UpdateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a service (soft delete)
    /// </summary>
    /// <param name="id">Service ID</param>
    /// <response code="204">Successfully deleted</response>
    /// <response code="404">Not found</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete service",
        Description = "Deletes a service from the catalog (soft delete)",
        OperationId = "DeleteService",
        Tags = new[] { "Services" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

