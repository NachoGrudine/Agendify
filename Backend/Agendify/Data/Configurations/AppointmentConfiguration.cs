using Agendify.Models.Entities;
using Agendify.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.StartTime)
            .IsRequired();
        
        builder.Property(a => a.EndTime)
            .IsRequired();
        
        builder.Property(a => a.Status)
            .IsRequired()
            .HasDefaultValue(AppointmentStatus.Pending);
        
        builder.Property(a => a.CreatedAt)
            .IsRequired();
        
        builder.Property(a => a.UpdatedAt);
        
        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Relaciones
        builder.HasOne(a => a.Business)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BusinessId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(a => a.Provider)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // �ndices para b�squedas eficientes
        builder.HasIndex(a => new { a.ProviderId, a.StartTime, a.EndTime });
        builder.HasIndex(a => new { a.BusinessId, a.StartTime });
        builder.HasIndex(a => a.Status);
    }
}

