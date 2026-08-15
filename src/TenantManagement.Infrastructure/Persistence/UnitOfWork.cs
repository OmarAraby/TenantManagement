using TenantManagement.Core.Interfaces;
using TenantManagement.Infrastructure.Persistence.Context;

namespace TenantManagement.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITenantRepository Tenants { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(AppDbContext context, ITenantRepository tenants, IUserRepository users)
    {
        _context = context;
        Tenants = tenants;
        Users = users;
    }



    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
