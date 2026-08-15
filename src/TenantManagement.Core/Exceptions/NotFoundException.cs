namespace TenantManagement.Core.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string resource)
        : base($"{resource} was not found.")
    {
        Resource = resource;
    }

    public string Resource { get; }
}
