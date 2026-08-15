using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TenantManagement.Core.Entities;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly ILookupNormalizer _normalizer;

    public UserRepository(AppDbContext context, ILookupNormalizer normalizer)
        : base(context)
    {
        _normalizer = normalizer;
    }

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = _normalizer.NormalizeEmail(email);

        return Entities.AnyAsync(
            user => user.NormalizedEmail == normalized
                && (excludingUserId == null || user.Id != excludingUserId),
            cancellationToken);
    }

    public override async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        ApplyNormalization(entity);
        await base.AddAsync(entity, cancellationToken);
    }

    public override void Update(User entity)
    {
        ApplyNormalization(entity);
        base.Update(entity);
    }

    private void ApplyNormalization(User user)
    {
        user.UserName = user.Email;
        user.NormalizedEmail = _normalizer.NormalizeEmail(user.Email);
        user.NormalizedUserName = _normalizer.NormalizeName(user.UserName);
    }
}
