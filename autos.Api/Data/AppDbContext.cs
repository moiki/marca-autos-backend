using System.Linq.Expressions;
using autos.Api.Features.MarcaAutos;
using Microsoft.EntityFrameworkCore;

namespace autos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Here all DbSets are defined by features
    public DbSet<MarcaAuto> MarcasAutos => Set<MarcaAuto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Read automatically all IEntityTypeConfiguration<T> assemblies.
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
