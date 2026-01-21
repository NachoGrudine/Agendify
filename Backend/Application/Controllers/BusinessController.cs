using Agendify.API.DTOs.Business;
using Agendify.API.Services;
using Agendify.API.Services.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<BusinessResponseDto>> Get()
    {
        var businessId = GetBusinessId();
        var business = await _businessService.GetByIdAsync(businessId);

        if (business == null)
        {
            return NotFound();
        }

        return Ok(business);
    }

    [HttpPut]
    public async Task<ActionResult<BusinessResponseDto>> Update([FromBody] UpdateBusinessDto dto)
    {
        var businessId = GetBusinessId();
        var business = await _businessService.UpdateAsync(businessId, dto);
        return Ok(business);
    }
}
