using TenantManagement.Core.Entities;

namespace TenantManagement.Core.Interfaces;

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
