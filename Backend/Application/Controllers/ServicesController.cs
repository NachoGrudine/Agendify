using Agendify.API.DTOs.Service;
using Agendify.API.Services;
using Agendify.API.Services.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
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
        var service = await _serviceService.GetByIdAsync(businessId, id);

        if (service == null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponseDto>> Create([FromBody] CreateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var service = await _serviceService.CreateAsync(businessId, dto);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResponseDto>> Update(int id, [FromBody] UpdateServiceDto dto)
    {
        var businessId = GetBusinessId();
        var service = await _serviceService.UpdateAsync(businessId, id, dto);
        return Ok(service);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        await _serviceService.DeleteAsync(businessId, id);
        return NoContent();
    }
}

