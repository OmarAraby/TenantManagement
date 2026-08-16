using TenantManagement.Core.Models;

namespace TenantManagement.Core.Interfaces;

public interface ITenantStats  //     to read taenant statistics outside http context 
{
    Task<IReadOnlyList<TenantUserCount>> GetActiveUserCountsAsync(
        CancellationToken cancellationToken = default);
}
