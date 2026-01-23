using Agendify.Common.Errors;
using Agendify.Models.Entities;
using Agendify.DTOs.Customer;
using Agendify.Repositories;
using FluentResults;

namespace Agendify.Services.Customers;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerService(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponseDto>> CreateAsync(int businessId, CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            BusinessId = businessId,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email
        };

        await _customerRepository.AddAsync(customer);
        return Result.Ok(MapToResponseDto(customer));
    }

    public async Task<Result<CustomerResponseDto>> UpdateAsync(int businessId, int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Cliente no encontrado"));
        }

        customer.Name = dto.Name;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;

        var updated = await _customerRepository.UpdateAsync(customer);
        return Result.Ok(MapToResponseDto(updated));
    }

    public async Task<Result<CustomerResponseDto>> GetByIdAsync(int businessId, int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            return Result.Fail<CustomerResponseDto>(new NotFoundError("Cliente no encontrado"));
        }

        return Result.Ok(MapToResponseDto(customer));
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetByBusinessAsync(int businessId)
    {
        var customers = await _customerRepository.FindAsync(c => c.BusinessId == businessId);
        return customers.Select(MapToResponseDto);
    }

    public async Task<Result> DeleteAsync(int businessId, int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            return Result.Fail(new NotFoundError("Cliente no encontrado"));
        }

        customer.IsDeleted = true;
        await _customerRepository.UpdateAsync(customer);
        
        return Result.Ok();
    }

    private static CustomerResponseDto MapToResponseDto(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            BusinessId = customer.BusinessId,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email
        };
    }
}

