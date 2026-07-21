using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventApi.Middleware;

/// <summary>
/// Adds the Bearer security requirement to each operation that is protected by
/// <see cref="AuthorizeAttribute"/> (and not opened up by
/// <see cref="AllowAnonymousAttribute"/>), so Swagger UI sends the Authorization
/// header for protected endpoints and leaves public ones open.
/// </summary>
public class SecurityRequirementsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (var api in context.ApiDescriptions)
        {
            var metadata = api.ActionDescriptor.EndpointMetadata;
            var isProtected =
                metadata.OfType<IAuthorizeData>().Any()
                && !metadata.OfType<IAllowAnonymous>().Any();
            if (!isProtected)
                continue;

            if (!TryGetOperation(document, api, out var operation) || operation is null)
                continue;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                }
            };
        }
    }

    // Matches an ApiDescription (relative path + HTTP method) to the operation
    // that Swashbuckle emitted for it in the document.
    private static bool TryGetOperation(
        OpenApiDocument document,
        Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription api,
        out OpenApiOperation? operation)
    {
        operation = null;
        if (api.RelativePath is null || api.HttpMethod is null)
            return false;

        var path = "/" + api.RelativePath.TrimStart('/');
        if (!document.Paths.TryGetValue(path, out var pathItem))
            return false;

        if (!Enum.TryParse<OperationType>(
                char.ToUpperInvariant(api.HttpMethod[0]) + api.HttpMethod[1..].ToLowerInvariant(),
                out var opType))
            return false;

        return pathItem.Operations.TryGetValue(opType, out operation);
    }
}
