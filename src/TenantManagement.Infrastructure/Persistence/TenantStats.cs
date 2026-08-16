using Microsoft.EntityFrameworkCore;
using TenantManagement.Core.Interfaces;
using TenantManagement.Core.Models;
using TenantManagement.Infrastructure.Persistence.Context;

namespace TenantManagement.Infrastructure.Persistence;

public sealed class TenantStats : ITenantStats
{
    private readonly AppDbContext _context;

    public TenantStats(AppDbContext context)
    {
        _context = context;
    }

    // for  each tenant, get the count of active users
    public async Task<IReadOnlyList<TenantUserCount>> GetActiveUserCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(tenant => tenant.Name)
            .Select(tenant => new TenantUserCount(
                tenant.Id,
                tenant.Name,
                tenant.Users.Count(user => user.IsActive)))
            .ToListAsync(cancellationToken);
    }
}
