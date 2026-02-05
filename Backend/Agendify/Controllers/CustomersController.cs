﻿using Agendify.DTOs.Customer;
using Agendify.Services.Customers;
using Agendify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendify.Controllers;

/// <summary>
/// Customers management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("Business customers management: registration, search and information management")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Gets all business customers
    /// </summary>
    /// <returns>List of customers</returns>
    /// <response code="200">List retrieved successfully</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "List all customers",
        Description = "Gets the complete list of customers from the authenticated business",
        OperationId = "GetAllCustomers",
        Tags = new[] { "Customers" }
    )]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
    {
        var businessId = GetBusinessId();
        var customers = await _customerService.GetByBusinessAsync(businessId);
        return Ok(customers);
    }

    /// <summary>
    /// Searches customers by name (partial, case-insensitive)
    /// </summary>
    /// <param name="name">Text to search</param>
    /// <returns>List of matching customers</returns>
    /// <response code="200">Search completed</response>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Search customers by name",
        Description = "Partial search of customers by name (useful for autocomplete)",
        OperationId = "SearchCustomers",
        Tags = new[] { "Customers" }
    )]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> Search([FromQuery] string name)
    {
        var businessId = GetBusinessId();
        var customers = await _customerService.SearchByNameAsync(businessId, name);
        return Ok(customers);
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer data</returns>
    /// <response code="200">Customer found</response>
    /// <response code="404">Not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get customer by ID",
        Description = "Returns complete information for a specific customer",
        OperationId = "GetCustomerById",
        Tags = new[] { "Customers" }
    )]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.GetByIdAsync(businessId, id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="dto">Customer data (name, email, phone, notes)</param>
    /// <returns>Created customer</returns>
    /// <response code="201">Successfully created</response>
    /// <response code="400">Invalid data or duplicate email</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new customer",
        Description = "Registers a new customer in the system",
        OperationId = "CreateCustomer",
        Tags = new[] { "Customers" }
    )]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.CreateAsync(businessId, dto);
        return result.ToCreatedResult(nameof(GetById), x => new { id = x.Id });
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="dto">New data</param>
    /// <returns>Updated customer</returns>
    /// <response code="200">Successfully updated</response>
    /// <response code="404">Not found</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update customer",
        Description = "Modifies existing customer data",
        OperationId = "UpdateCustomer",
        Tags = new[] { "Customers" }
    )]
    public async Task<ActionResult<CustomerResponseDto>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.UpdateAsync(businessId, id, dto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a customer (soft delete)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <response code="204">Successfully deleted</response>
    /// <response code="404">Not found</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete customer",
        Description = "Deletes a customer from the system (soft delete)",
        OperationId = "DeleteCustomer",
        Tags = new[] { "Customers" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var businessId = GetBusinessId();
        var result = await _customerService.DeleteAsync(businessId, id);
        return result.ToActionResult();
    }
}

