using Agendify.DTOs.Service;
using Agendify.Services.ServicesServices;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : BaseController
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var services = await _serviceService.GetByBusinessAsync(businessId);
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponseDto>> Create([FromBody] CreateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResponseDto>> Update(int id, [FromBody] UpdateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _serviceService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

