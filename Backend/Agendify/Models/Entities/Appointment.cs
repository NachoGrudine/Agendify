using Agendify.Models.Enums;

namespace Agendify.Models.Entities;

public class Appointment : BaseEntity
{
    public int BusinessId { get; set; }
    public int ProviderId { get; set; }
    public int? CustomerId { get; set; }
    public int? ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
    public Provider? Provider { get; set; }
    public Customer? Customer { get; set; }
    public Service? Service { get; set; }
}

