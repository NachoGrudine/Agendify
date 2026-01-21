using Agendify.API.DTOs.Provider;
using Agendify.API.Services;
using Agendify.API.Services.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProviderResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var providers = await _providerService.GetByBusinessAsync(businessId);
        return Ok(providers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProviderResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var provider = await _providerService.GetByIdAsync(businessId, id);

        if (provider == null)
        {
            return NotFound();
        }

        return Ok(provider);
    }

    [HttpPost]
    public async Task<ActionResult<ProviderResponseDto>> Create([FromBody] CreateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var provider = await _providerService.CreateAsync(businessId, dto);
        return CreatedAtAction(nameof(GetById), new { id = provider.Id }, provider);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProviderResponseDto>> Update(int id, [FromBody] UpdateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var provider = await _providerService.UpdateAsync(businessId, id, dto);
        return Ok(provider);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        await _providerService.DeleteAsync(businessId, id);
        return NoContent();
    }
}

