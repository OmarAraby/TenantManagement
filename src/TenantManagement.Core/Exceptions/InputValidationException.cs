namespace TenantManagement.Core.Exceptions;

public sealed class InputValidationException : DomainException
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public InputValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public InputValidationException(string message)
        : base(message)
    {
        Errors = NoErrors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
