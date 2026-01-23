using Agendify.Models.Entities;

namespace Agendify.DTOs.Appointment;

public class CreateAppointmentDto
{
    public int ProviderId { get; set; }
    public int? CustomerId { get; set; }
    public int? ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
