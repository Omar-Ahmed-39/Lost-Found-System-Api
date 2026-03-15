using System.Net;
using System.Text.Json;

namespace LostAndFound.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext content)
    {
        try
        {
            await _next(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            content.Response.ContentType = "application/json";
            content.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                Succeeded = false,
                Message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred , please try again later.",
                Errors = _env.IsDevelopment() ? new { ex.StackTrace } : null
            };

            var option = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, option);

            await content.Response.WriteAsync(json);
        }
    }
}