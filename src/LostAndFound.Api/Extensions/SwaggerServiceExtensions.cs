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
                              "Enter only your JWT token in the text input below.\r\n\r\n" +
                              "Example: \"eyJhbGci...\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Apply a valid Bearer requirement reference for generated operations.
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document, null),
                    new List<string>()
                }
            });
        });

        return services;
    }
}
