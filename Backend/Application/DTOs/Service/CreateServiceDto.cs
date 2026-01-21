namespace Agendify.API.DTOs.Service;

public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public int DefaultDuration { get; set; }
    public decimal Price { get; set; }
}
