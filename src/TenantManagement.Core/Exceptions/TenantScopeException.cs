namespace TenantManagement.Core.Exceptions;

public sealed class TenantScopeException : DomainException
{
    public TenantScopeException(string message)
        : base(message)
    {
    }
}
