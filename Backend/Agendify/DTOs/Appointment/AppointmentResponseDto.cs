using Agendify.Models.Enums;

namespace Agendify.DTOs.Appointment;

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
}

