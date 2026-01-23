using Agendify.DTOs.Provider;
using Agendify.Services.Providers;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController : BaseController
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
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
        var result = await _providerService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<ProviderResponseDto>> Create([FromBody] CreateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProviderResponseDto>> Update(int id, [FromBody] UpdateProviderDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _providerService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

