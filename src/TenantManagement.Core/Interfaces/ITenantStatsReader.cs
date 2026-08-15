using TenantManagement.Core.Models;

namespace TenantManagement.Core.Interfaces;

public interface ITenantStatsReader
{
    Task<IReadOnlyList<TenantUserCount>> GetActiveUserCountsAsync(
        CancellationToken cancellationToken = default);
}
