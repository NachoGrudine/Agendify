using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Agendify.Data;

public class AgendifyDbContext : DbContext
{
    public AgendifyDbContext(DbContextOptions<AgendifyDbContext> options) : base(options)
    {
    }
    
    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<ProviderSchedule> ProviderSchedules { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplicar todas las configuraciones automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Query Filters globales para Soft Delete
        // NO aplicamos query filter a Business porque User tiene relación requerida con Business
        modelBuilder.Entity<Provider>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<ProviderSchedule>().HasQueryFilter(ps => !ps.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Service>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Configurar automáticamente CreatedAt y UpdatedAt
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}

