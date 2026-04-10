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
        catch (UnauthorizedAccessException ex)
        {
            // Thrown by FirebaseAuthService for invalid/expired tokens
            _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
            await WriteJsonResponseAsync(content, (int)HttpStatusCode.Unauthorized, "Unauthorized.", ex);
        }
        catch (NotSupportedException ex)
        {
            // Thrown by FirebaseAuthService for unsupported operations (e.g. Register, RefreshToken)
            _logger.LogWarning(ex, "Not supported: {Message}", ex.Message);
            await WriteJsonResponseAsync(content, (int)HttpStatusCode.BadRequest, ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteJsonResponseAsync(content, (int)HttpStatusCode.InternalServerError,
                _env.IsDevelopment() ? ex.Message : "An unexpected error occurred, please try again later.", ex);
        }
    }

    private async Task WriteJsonResponseAsync(HttpContext context, int statusCode, string message, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            Succeeded = false,
            Message = message,
            Errors = _env.IsDevelopment() ? new { ex.StackTrace } : null
        };

        var option = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, option));
    }
}