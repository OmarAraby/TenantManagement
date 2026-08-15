using TenantManagement.Core.Exceptions;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.MultiTenancy;

public sealed class TenantContext : ITenantContext, ITenantContextSetter
{
    public Guid? TenantId { get; private set; }

    public bool HasTenant => TenantId.HasValue;

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new TenantScopeException("A tenant identifier is required.");
        }

        if (TenantId.HasValue && TenantId.Value != tenantId)
        {
            throw new TenantScopeException("The tenant scope has already been established and cannot be changed.");
        }

        TenantId = tenantId;
    }
}
