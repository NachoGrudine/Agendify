namespace Agendify.API.DTOs.Service;

public class ServiceResponseDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DefaultDuration { get; set; }
    public decimal Price { get; set; }
}

