using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TenantManagement.Application.Interfaces;
using TenantManagement.Application.Services;

namespace TenantManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUserService, UserService>();

        // validations
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
