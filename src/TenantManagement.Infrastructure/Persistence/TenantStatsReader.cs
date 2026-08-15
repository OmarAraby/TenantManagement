using Microsoft.EntityFrameworkCore;
using TenantManagement.Core.Interfaces;
using TenantManagement.Core.Models;

namespace TenantManagement.Infrastructure.Persistence;

public sealed class TenantStatsReader : ITenantStatsReader
{
    private readonly AppDbContext _context;

    public TenantStatsReader(AppDbContext context)
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
