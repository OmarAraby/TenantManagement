using TenantManagement.Core.Enums;

namespace TenantManagement.Application.DTOs.Users;

public sealed record UpdateUserRequest(string FullName, string Email, UserRole Role);
