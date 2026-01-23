using Agendify.DTOs.Business;
using Agendify.Services.Business;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : BaseController
{
    private readonly IBusinessService _businessService;
    

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }


    [HttpGet]
    public async Task<ActionResult<BusinessResponseDto>> Get()
    {
        var businessId = GetBusinessId();
        var result = await _businessService.GetByIdAsync(businessId);
        return result.ToActionResult();
    }

    [HttpPut]
    public async Task<ActionResult<BusinessResponseDto>> Update([FromBody] UpdateBusinessDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _businessService.UpdateAsync(businessId, dto);
        return result.ToActionResult();
    }
}
