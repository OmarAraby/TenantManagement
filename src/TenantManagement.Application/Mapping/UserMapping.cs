using TenantManagement.Application.DTOs.Users;
using TenantManagement.Core.Entities;

namespace TenantManagement.Application.Mapping;

public static class UserMapping
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse(
            user.Id,
            user.TenantId,
            user.FullName,
            user.Email ?? string.Empty,
            user.Role,
            user.IsActive,
            user.CreatedAt);
    }

    public static IReadOnlyList<UserResponse> ToResponses(this IEnumerable<User> users)
    {
        return users.Select(ToResponse).ToList();
    }
}
