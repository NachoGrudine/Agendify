using Agendify.Models.Entities;
using Agendify.DTOs.Customer;
using Agendify.Repositories;

namespace Agendify.Services.Customers;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerService(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerResponseDto> CreateAsync(int businessId, CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            BusinessId = businessId,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email
        };

        await _customerRepository.AddAsync(customer);
        return MapToResponseDto(customer);
    }

    public async Task<CustomerResponseDto> UpdateAsync(int businessId, int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Cliente no encontrado");
        }

        customer.Name = dto.Name;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;

        var updated = await _customerRepository.UpdateAsync(customer);
        return MapToResponseDto(updated);
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int businessId, int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            return null;
        }

        return MapToResponseDto(customer);
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetByBusinessAsync(int businessId)
    {
        var customers = await _customerRepository.FindAsync(c => c.BusinessId == businessId);
        return customers.Select(MapToResponseDto);
    }

    public async Task DeleteAsync(int businessId, int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.BusinessId != businessId)
        {
            throw new KeyNotFoundException("Cliente no encontrado");
        }

        customer.IsDeleted = true;
        await _customerRepository.UpdateAsync(customer);
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

