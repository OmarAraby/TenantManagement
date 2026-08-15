namespace TenantManagement.Core.Exceptions;

public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
