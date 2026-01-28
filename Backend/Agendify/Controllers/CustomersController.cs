﻿using Agendify.DTOs.Customer;
using Agendify.Services.Customers;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agendify.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var customers = await _customerService.GetByBusinessAsync(businessId);
        return Ok(customers);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> Search([FromQuery] string name)
    {
        var businessId = GetBusinessId();
        var customers = await _customerService.SearchByNameAsync(businessId, name);
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

