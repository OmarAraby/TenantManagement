using TenantManagement.Application.DTOs.Tenants;

namespace TenantManagement.Application.Interfaces;

public interface ITenantService
{
    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);

    Task<TenantResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
