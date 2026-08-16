using TenantManagement.Application.DTOs.Tenants;
using TenantManagement.Core.Entities;

namespace TenantManagement.Application.Mapping;

public static class TenantMapping
{
    public static TenantResponse ToResponse(this Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive,
            tenant.CreatedAt);
    }
}
