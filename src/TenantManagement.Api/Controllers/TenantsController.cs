using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TenantManagement.Application.Common;
using TenantManagement.Application.DTOs.Tenants;
using TenantManagement.Application.Interfaces;

namespace TenantManagement.Api.Controllers;

[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<Created<ApiResponse<TenantResponse>>> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/tenants/{tenant.Id}",
            ApiResponse<TenantResponse>.SuccessResponse(tenant, "Tenant created successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<Ok<ApiResponse<TenantResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);

        return TypedResults.Ok(ApiResponse<TenantResponse>.SuccessResponse(tenant, "Tenant retrieved successfully"));
    }
}
