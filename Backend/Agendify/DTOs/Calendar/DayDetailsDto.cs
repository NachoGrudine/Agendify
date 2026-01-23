using Agendify.DTOs.Appointment;

namespace Agendify.DTOs.Calendar;

public class DayDetailsDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int TotalScheduledMinutes { get; set; }
    public int TotalOccupiedMinutes { get; set; }
    public List<AppointmentDetailDto> Appointments { get; set; } = new();
}

