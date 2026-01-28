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
        
        // Versionado temporal
        builder.Property(ps => ps.ValidFrom)
            .IsRequired();
        
        builder.Property(ps => ps.ValidUntil);
        
        // Índice para búsqueda eficiente por día de semana
        builder.HasIndex(ps => new { ps.ProviderId, ps.DayOfWeek });
        
        // Índice para consultas temporales eficientes
        builder.HasIndex(ps => new { ps.ProviderId, ps.ValidFrom, ps.ValidUntil });
    }
}

