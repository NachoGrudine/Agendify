﻿using Agendify.DTOs.Customer;

namespace Agendify.Services.Customers;

public interface ICustomerService
{
    Task<CustomerResponseDto> CreateAsync(int businessId, CreateCustomerDto dto);
    Task<CustomerResponseDto> UpdateAsync(int businessId, int id, UpdateCustomerDto dto);
    Task<CustomerResponseDto?> GetByIdAsync(int businessId, int id);
    Task<IEnumerable<CustomerResponseDto>> GetByBusinessAsync(int businessId);
    Task DeleteAsync(int businessId, int id);
}
