using Agendify.DTOs.Customer;
using Agendify.Services.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    private int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var customers = await _customerService.GetByBusinessAsync(businessId);
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var customer = await _customerService.GetByIdAsync(businessId, id);

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var customer = await _customerService.CreateAsync(businessId, dto);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var customer = await _customerService.UpdateAsync(businessId, id, dto);
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        await _customerService.DeleteAsync(businessId, id);
        return NoContent();
    }
}

