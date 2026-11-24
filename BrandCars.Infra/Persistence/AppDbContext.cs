using System.Linq.Expressions;
using BrandCars.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrandCars.Infra.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MarcaAuto> MarcasAutos => Set<MarcaAuto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Descubre todas las IEntityTypeConfiguration<T> del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildIsActiveFilter(entityType.ClrType));
            }
        }
    }
    private static LambdaExpression BuildIsActiveFilter(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(ISoftDeletable.IsActive));
        var body = Expression.Equal(prop, Expression.Constant(true));
        return Expression.Lambda(body, param);
    }
}
