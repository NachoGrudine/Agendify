using Agendify.DTOs.Customer;
using FluentResults;

namespace Agendify.Services.Customers;

public interface ICustomerService
{
    Task<Result<CustomerResponseDto>> CreateAsync(int businessId, CreateCustomerDto dto);
    Task<Result<CustomerResponseDto>> UpdateAsync(int businessId, int id, UpdateCustomerDto dto);
    Task<Result<CustomerResponseDto>> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<CustomerResponseDto>> GetByBusinessAsync(int businessId);
    Task<IEnumerable<CustomerResponseDto>> SearchByNameAsync(int businessId, string name);
    Task<Result> DeleteAsync(int businessId, int id);
}
