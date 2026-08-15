namespace TenantManagement.Core.Interfaces;

public interface IUnitOfWork
{
    ITenantRepository Tenants { get; }

    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
