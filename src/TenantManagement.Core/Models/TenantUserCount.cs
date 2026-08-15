namespace TenantManagement.Core.Models;

public sealed record TenantUserCount(Guid TenantId, string TenantName, int ActiveUserCount);
