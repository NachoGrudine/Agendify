namespace Agendify.API.Domain.Entities;

public class Provider : BaseEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<ProviderSchedule> Schedules { get; set; } = new List<ProviderSchedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

