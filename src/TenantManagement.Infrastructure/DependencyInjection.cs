using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Core.Interfaces;
using TenantManagement.Infrastructure.MultiTenancy;
using TenantManagement.Infrastructure.Persistence;
using TenantManagement.Infrastructure.Persistence.Repositories;

namespace TenantManagement.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextSetter>(provider => provider.GetRequiredService<TenantContext>()); // for mdw to set the tenant context

        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();  // Register the default implementation of ILookupNormalizer --> that related to  the IdentityUser normalization

        // Register repositories and unit of work 
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register the TenantStatsReader service 
        services.AddScoped<ITenantStatsReader, TenantStatsReader>();  //for reading tenant stats that are not part of the unit of work and for hangfire jobs that has no context of the current tenant 

        return services;
    }
}
