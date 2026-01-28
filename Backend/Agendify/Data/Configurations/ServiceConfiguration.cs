using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(s => s.DefaultDuration)
            .IsRequired();
        
        builder.Property(s => s.Price)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.CreatedAt)
            .IsRequired();
        
        builder.Property(s => s.UpdatedAt);
        
        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}

