using EntityFramework.Exceptions.PostgreSQL;
using FinanceManager.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceManager.Api.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseExceptionProcessor();
        optionsBuilder.ConfigureWarnings(x => x.Log(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            var hasAutoIncrEntity = Array.Find(entityType.ClrType.GetInterfaces(),
                i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAutoIncrementalEntity<>));

            if (hasAutoIncrEntity is not null)
            {
                const string propertyName = nameof(IAutoIncrementalEntity<>.Id);

                var property = entityType.ClrType.GetProperty(propertyName);

                if (property is not null)
                {
                    modelBuilder.Entity(entityType.ClrType).Property(property.PropertyType, propertyName);
                }

                var entityBuilder = modelBuilder.Entity(entityType.ClrType);
                entityBuilder.Property(propertyName).UseIdentityAlwaysColumn();
                entityBuilder.HasKey(propertyName);
            }
        }
    }
}