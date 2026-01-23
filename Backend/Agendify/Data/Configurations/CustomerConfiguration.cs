using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(c => c.Phone)
            .HasMaxLength(20);
        
        builder.Property(c => c.Email)
            .HasMaxLength(255);
        
        builder.Property(c => c.CreatedAt)
            .IsRequired();
        
        builder.Property(c => c.UpdatedAt);
        
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // �ndice para b�squeda por email dentro del negocio (solo si tiene email)
        builder.HasIndex(c => new { c.BusinessId, c.Email });
    }
}

