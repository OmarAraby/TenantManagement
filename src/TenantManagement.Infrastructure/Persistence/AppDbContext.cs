using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TenantManagement.Core.Entities;
using TenantManagement.Core.Exceptions;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Infrastructure.Persistence;

public class AppDbContext : IdentityUserContext<User, Guid>
{
    private const string CreatedAtProperty = nameof(Tenant.CreatedAt);

    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public Guid? CurrentTenantId => _tenantContext.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantAndAuditRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantAndAuditRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);



        // global query to scope users to the current tenant and only active users
        builder.Entity<User>()
            .HasQueryFilter(u => u.TenantId == CurrentTenantId && u.IsActive);
    }

    private void ApplyTenantAndAuditRules()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                StampCreatedAt(entry, now);
            }
        }

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    AssignTenant(entry);
                    break;

                case EntityState.Modified:
                case EntityState.Deleted:
                    GuardTenant(entry);
                    break;
            }
        }
    }

    private static void StampCreatedAt(EntityEntry entry, DateTime now)
    {
        var property = entry.Metadata.FindProperty(CreatedAtProperty);

        if (property is null || property.ClrType != typeof(DateTime))
        {
            return;
        }

        var value = entry.Property(CreatedAtProperty);

        if (value.CurrentValue is DateTime current && current == default)
        {
            value.CurrentValue = now;
        }
    }


    // ensure that user is related to the current tenant when creating a new user, and that the user cannot be moved to another tenant or modified outside the current tenant
    private void AssignTenant(EntityEntry<User> entry)
    {
        if (_tenantContext.TenantId is not { } tenantId)
        {
            throw new TenantScopeException("A tenant scope is required to create a user.");
        }

        if (entry.Entity.TenantId == Guid.Empty)
        {
            entry.Entity.TenantId = tenantId;
            return;
        }

        if (entry.Entity.TenantId != tenantId)
        {
            throw new ForbiddenException("A user cannot be created outside the current tenant.");
        }
    }



    // user alread y exists, ensure that the user cannot be moved to another tenant or modified outside the current tenant
    private void GuardTenant(EntityEntry<User> entry)
    {
        if (_tenantContext.TenantId is not { } tenantId)
        {
            throw new TenantScopeException("A tenant scope is required to modify a user.");
        }

        var tenantProperty = entry.Property(u => u.TenantId);

        if (tenantProperty.IsModified && !Equals(tenantProperty.OriginalValue, tenantProperty.CurrentValue))
        {
            throw new ForbiddenException("A user cannot be moved to another tenant.");
        }

        if (!Equals(tenantProperty.OriginalValue, tenantId))
        {
            throw new ForbiddenException("A user outside the current tenant cannot be modified.");
        }
    }
}
