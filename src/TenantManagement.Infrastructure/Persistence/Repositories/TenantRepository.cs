using Microsoft.EntityFrameworkCore;
using TenantManagement.Core.Entities;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return Entities.AnyAsync(tenant => tenant.Slug == slug, cancellationToken);
    }
}
