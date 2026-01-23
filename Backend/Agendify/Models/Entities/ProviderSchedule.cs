namespace Agendify.Models.Entities;

public class ProviderSchedule : BaseEntity
{
    public int ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    // Navigation properties
    public Provider? Provider { get; set; }
}

