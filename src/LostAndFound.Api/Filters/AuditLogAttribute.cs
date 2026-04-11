using System.Security.Claims;
using LostAndFound.Core.Entities;
using LostAndFound.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Api.Filters;

public class AuditLogAttribute : TypeFilterAttribute
{
    public AuditLogAttribute(string actionName) : base(typeof(AuditLogFilter))
    {
        Arguments = new object[] { actionName };
    }
}

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly string _actionName;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogFilter> _logger;

    public AuditLogFilter(string actionName, IUnitOfWork unitOfWork, ILogger<AuditLogFilter> logger)
    {
        _actionName = actionName;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (resultContext.Exception == null)
        {
            try
            {
                var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

                // try to extract Target 
                context.RouteData.Values.TryGetValue("id", out var targetId);
                string targetDetails = targetId != null ? $"Entity ID: {targetId}" : "System/General";

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = _actionName,
                    Target = targetDetails,
                    IpAddress = ipAddress ?? "Unknown",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit log.");
            }
        }
    }
}