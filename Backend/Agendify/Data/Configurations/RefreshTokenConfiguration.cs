using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agendify.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
        
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
        
        builder.Property(rt => rt.RevokedAt);
        
        builder.Property(rt => rt.DeviceInfo)
            .HasMaxLength(500);
        
        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(50);
        
        // Índice para buscar tokens activos por usuario
        builder.HasIndex(rt => new { rt.UserId, rt.Token });
        
        // Índice para limpiar tokens expirados
        builder.HasIndex(rt => rt.ExpiresAt);
        
        // Relación con User
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

