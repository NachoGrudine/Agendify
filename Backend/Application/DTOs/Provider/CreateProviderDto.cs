namespace Agendify.API.DTOs.Provider;

public class CreateProviderDto
{
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
