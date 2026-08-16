using TenantManagement.Core.Enums;

namespace TenantManagement.Application.DTOs.Users;

public sealed record UserResponse(
    Guid Id,
    Guid TenantId,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt);
