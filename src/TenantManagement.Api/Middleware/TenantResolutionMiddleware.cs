using TenantManagement.Core.Exceptions;
using TenantManagement.Core.Interfaces;

namespace TenantManagement.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContextSetter tenantContextSetter)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var value = values.ToString();

            if (!Guid.TryParse(value, out var tenantId))
            {
                throw new TenantScopeException($"The {HeaderName} header is not a valid identifier.");
            }

            tenantContextSetter.SetTenant(tenantId);
        }

        await _next(context);
    }
}
