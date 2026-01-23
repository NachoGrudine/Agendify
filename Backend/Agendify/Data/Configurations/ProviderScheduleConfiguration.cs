using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class ProviderScheduleConfiguration : IEntityTypeConfiguration<ProviderSchedule>
{
    public void Configure(EntityTypeBuilder<ProviderSchedule> builder)
    {
        builder.ToTable("ProviderSchedules");
        
        builder.HasKey(ps => ps.Id);
        
        builder.Property(ps => ps.DayOfWeek)
            .IsRequired();
        
        builder.Property(ps => ps.StartTime)
            .IsRequired();
        
        builder.Property(ps => ps.EndTime)
            .IsRequired();
        
        builder.Property(ps => ps.CreatedAt)
            .IsRequired();
        
        builder.Property(ps => ps.UpdatedAt);
        
        builder.Property(ps => ps.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // �ndice para b�squeda eficiente
        builder.HasIndex(ps => new { ps.ProviderId, ps.DayOfWeek });
}
    }

