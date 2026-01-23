﻿namespace Agendify.DTOs.Provider;

public class UpdateProviderDto
{
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

