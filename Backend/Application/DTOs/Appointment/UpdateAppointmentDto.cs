﻿﻿﻿using Agendify.API.Domain.Entities;

namespace Agendify.API.DTOs.Appointment;

public class UpdateAppointmentDto
{
    public int ProviderId { get; set; }
    public int? CustomerId { get; set; }
    public int? ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
}

