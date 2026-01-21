﻿namespace Agendify.API.DTOs.Customer;

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
