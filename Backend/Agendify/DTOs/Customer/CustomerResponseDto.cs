﻿namespace Agendify.DTOs.Customer;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

