using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TenantManagement.Api.Filters;
using TenantManagement.Api.Middleware;
using TenantManagement.Api.OpenApi;
using TenantManagement.Application;
using TenantManagement.Core.Exceptions;
using TenantManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();


builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));  // MVC input formatter for enums

// minimal APIs input formatter for enums
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));



builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => throw new InputValidationException(
        context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => IsDeserializationError(entry.Key, error)
                        ? "The value provided is not valid."
                        : error.ErrorMessage)
                    .ToArray()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.OperationFilter<TenantHeaderOperationFilter>());

var app = builder.Build();

// First in the pipeline so it catches everything downstream, including tenant resolution.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

static bool IsDeserializationError(string key, ModelError error)
{
    return error.Exception is not null || key.StartsWith('$');
}
