using TenantManagement.Core.Enums;

namespace TenantManagement.Application.DTOs.Users;

public sealed record CreateUserRequest(string FullName, string Email, UserRole Role);
