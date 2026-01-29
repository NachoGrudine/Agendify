using Agendify.DTOs.Appointment;

namespace Agendify.DTOs.Calendar;

public class DayDetailsDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int AppointmentsTrend { get; set; } // Diferencia con el día anterior (ej: +2, -3, 0)
    public int TotalScheduledMinutes { get; set; }
    public int TotalOccupiedMinutes { get; set; }
    public List<AppointmentDetailDto> Appointments { get; set; } = new();

    // Paginación
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

