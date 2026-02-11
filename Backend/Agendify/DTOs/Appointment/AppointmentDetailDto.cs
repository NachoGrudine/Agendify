namespace Agendify.DTOs.Appointment;

public class AppointmentDetailDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ServiceName { get; set; }
    public string StartTime { get; set; } = string.Empty;  // HH:mm format
    public string EndTime { get; set; } = string.Empty;    // HH:mm format
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
}
