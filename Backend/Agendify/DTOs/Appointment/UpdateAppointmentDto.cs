namespace Agendify.DTOs.Appointment;

public class UpdateAppointmentDto
{
    public int ProviderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
}

