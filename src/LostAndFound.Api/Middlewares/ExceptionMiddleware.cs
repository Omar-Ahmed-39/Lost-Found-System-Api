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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Thrown by FirebaseAuthService for invalid/expired tokens, or by BaseController.GetUserId()
            _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
            await WriteJsonResponseAsync(context, (int)HttpStatusCode.Unauthorized, "Unauthorized.", ex);
        }
        catch (NotSupportedException ex)
        {
            // Thrown by FirebaseAuthService for unsupported operations (e.g. Register, RefreshToken)
            _logger.LogWarning(ex, "Not supported: {Message}", ex.Message);
            await WriteJsonResponseAsync(context, (int)HttpStatusCode.BadRequest, ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            var message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred, please try again later.";
            await WriteJsonResponseAsync(context, (int)HttpStatusCode.InternalServerError, message, ex);
        }
    }

    private async Task WriteJsonResponseAsync(HttpContext context, int statusCode, string message, Exception ex)
    {
        // Guard: if headers are already flushed, we cannot write a new response.
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started; cannot write error JSON for: {Message}", message);
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // Use the same ApiResponse<T> shape as the rest of the API for consistency.
        var errors = _env.IsDevelopment()
            ? new List<string> { ex.StackTrace ?? ex.Message }
            : new List<string> { message };

        var response = ApiResponse<object>.Failure(errors, message);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}