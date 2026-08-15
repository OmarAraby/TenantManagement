namespace TenantManagement.Core.Interfaces;

public interface ITenantContextSetter
{
    void SetTenant(Guid tenantId);
}
