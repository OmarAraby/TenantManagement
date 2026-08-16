using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using TenantManagement.Api.Middleware;

namespace TenantManagement.Api.OpenApi;
// for swgger in test dev env
public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = TenantResolutionMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Identifier of the tenant the request runs against.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });
    }
}
