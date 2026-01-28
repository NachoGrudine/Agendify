namespace Agendify.Models.Entities;

public class Service : BaseEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DefaultDuration { get; set; }
    public decimal? Price { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

