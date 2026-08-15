namespace TenantManagement.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
