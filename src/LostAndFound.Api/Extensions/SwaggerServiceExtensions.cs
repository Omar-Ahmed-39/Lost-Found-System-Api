using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LostAndFound.Api.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Lost and Found API",
                Version = "v1",
                Description = "RESTful API for the Lost and Found management system."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                              "Enter 'Bearer' [space] and your token in the text input below.\r\n\r\n" +
                              "Example: \"Bearer eyJhbGci...\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Apply the security lock only to endpoints decorated with [Authorize],
            // skipping any [AllowAnonymous] endpoints (e.g., Login, Register).
            c.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        return services;
    }
}

/// <summary>
/// Swagger operation filter that conditionally applies the Bearer security requirement
/// only to endpoints that have an effective [Authorize] attribute and no [AllowAnonymous].
/// This prevents the padlock icon from appearing on public endpoints in Swagger UI.
/// </summary>
internal sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (hasAllowAnonymous) return;

        var hasAuthorize = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!hasAuthorize) return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null!, null),
                    new List<string>()
                }
            }
        };
    }
}
