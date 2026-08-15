using Microsoft.AspNetCore.Identity;
using TenantManagement.Core.Enums;

namespace TenantManagement.Core.Entities;

public class User : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }

    public required string FullName { get; set; }

    public UserRole Role { get; set; } = UserRole.Member;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
