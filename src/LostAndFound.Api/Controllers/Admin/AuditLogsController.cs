using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LostAndFound.Api.DTOs.Admin;
using LostAndFound.Core.Constants;
using LostAndFound.Infrastructure;

namespace LostAndFound.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class AuditLogsController : BaseController
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet(ApiRoutes.AuditLogs.GetAll)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _dbContext.AuditLogs
            .Include(a => a.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(a => 
                a.Action.ToLower().Contains(term) ||
                a.Target.ToLower().Contains(term) ||
                (a.User != null && a.User.Name.ToLower().Contains(term)));
        }

        var totalRecords = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoLogs = logs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            Timestamp = a.CreatedAt,
            AdminName = a.User?.Name ?? "System",
            Action = a.Action,
            Target = a.Target,
            IpAddress = a.IpAddress
        }).ToList();

        return Paged((IEnumerable<AuditLogDto>)dtoLogs, page, pageSize, totalRecords);
    }
}
