namespace Agendify.Models.Entities;

public class Customer : BaseEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

