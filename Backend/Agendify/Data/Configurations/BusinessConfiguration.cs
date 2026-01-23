using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(b => b.Industry)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(b => b.CreatedAt)
            .IsRequired();
        
        builder.Property(b => b.UpdatedAt);
        
        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Relaciones
        builder.HasMany(b => b.Providers)
            .WithOne(p => p.Business)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(b => b.Customers)
            .WithOne(c => c.Business)
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(b => b.Services)
            .WithOne(s => s.Business)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

