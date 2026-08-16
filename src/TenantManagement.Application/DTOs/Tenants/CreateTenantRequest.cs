namespace TenantManagement.Application.DTOs.Tenants;

public sealed record CreateTenantRequest(string Name, string Slug);
