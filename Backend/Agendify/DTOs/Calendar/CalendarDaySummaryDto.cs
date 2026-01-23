namespace Agendify.DTOs.Calendar;

public class CalendarDaySummaryDto
{
    public DateTime Date { get; set; }
    public int AppointmentsCount { get; set; }
    public int TotalScheduledMinutes { get; set; }  // Tiempo total que los providers trabajan ese día
    public int TotalOccupiedMinutes { get; set; }   // Tiempo ocupado por appointments
    public int TotalAvailableMinutes { get; set; }  // TotalScheduled - TotalOccupied
}
