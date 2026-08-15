namespace TenantManagement.Core.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }

    bool HasTenant { get; }
}
