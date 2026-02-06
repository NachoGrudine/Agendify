namespace Agendify.DTOs.Appointment;

/// <summary>
/// Datos del próximo turno programado
/// </summary>
public class NextAppointmentResponseDto
{
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Hora de inicio del turno
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Hora de fin del turno
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Día del turno (fecha completa)
    /// </summary>
    public DateTime Day { get; set; }
}

