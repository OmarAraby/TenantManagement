using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    private const string IdProperty = "Id";

    public GenericRepository(AppDbContext context)
    {
        Context = context;
    }

    protected AppDbContext Context { get; }

    protected DbSet<T> Entities => Context.Set<T>();

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(
            entity => EF.Property<Guid>(entity, IdProperty) == id,
            cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public virtual Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Entities.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return Entities.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        Entities.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        Entities.Remove(entity);
    }
}
