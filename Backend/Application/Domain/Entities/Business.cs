namespace Agendify.API.Domain.Entities;

public class Business : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<Provider> Providers { get; set; } = new List<Provider>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

