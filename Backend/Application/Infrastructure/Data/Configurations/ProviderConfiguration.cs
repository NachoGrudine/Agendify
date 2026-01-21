using Agendify.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.API.Infrastructure.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Specialty)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt);
        
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Relaciones
        builder.HasMany(p => p.Schedules)
            .WithOne(s => s.Provider)
            .HasForeignKey(s => s.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

