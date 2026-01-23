﻿namespace Agendify.DTOs.Service;

public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public int DefaultDuration { get; set; }
    public decimal Price { get; set; }
}

