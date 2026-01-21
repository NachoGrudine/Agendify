﻿﻿using Agendify.API.Domain.Entities;

namespace Agendify.API.DTOs.Appointment;

public class CreateAppointmentDto
{
    public int ProviderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
