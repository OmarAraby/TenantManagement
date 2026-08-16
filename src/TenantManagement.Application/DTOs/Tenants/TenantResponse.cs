namespace TenantManagement.Application.DTOs.Tenants;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAt);
