using TenantManagement.Core.Entities;

namespace TenantManagement.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> EmailExistsAsync(
        string email,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);
}
