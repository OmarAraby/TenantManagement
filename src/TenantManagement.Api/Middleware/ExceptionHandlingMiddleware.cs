using TenantManagement.Application.Common;
using TenantManagement.Core.Exceptions;

namespace TenantManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception, "Response already started; cannot write an error payload.");
            throw exception;
        }

        var statusCode = ResolveStatusCode(exception);
        var errors = ResolveErrors(exception);

        var message = exception is DomainException
            ? exception.Message
            : "An unexpected error occurred.";

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (_environment.IsDevelopment())
            {
                errors.Add(exception.ToString());
            }
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            ApiResponse.ErrorResponse(message, errors),
            context.RequestAborted);
    }

    private static int ResolveStatusCode(Exception exception) => exception switch
    {
        InputValidationException => StatusCodes.Status400BadRequest,
        TenantScopeException => StatusCodes.Status400BadRequest,
        ForbiddenException => StatusCodes.Status403Forbidden,
        NotFoundException => StatusCodes.Status404NotFound,
        ConflictException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static List<string> ResolveErrors(Exception exception)
    {
        if (exception is not InputValidationException validationException)
        {
            return [];
        }

        return validationException.Errors
            .SelectMany(entry => entry.Value.Select(message => $"{entry.Key}: {message}"))
            .ToList();
    }
}
