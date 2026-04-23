using LostAndFound.Core.Constants;
using Microsoft.AspNetCore.Mvc;
using LostAndFound.Core.Interfaces;
using LostAndFound.Core.Domain.Dashboard;
using LostAndFound.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace LostAndFound.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
public class DashboardController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet(ApiRoutes.Dashboard.GetStats)]
    public async Task<IActionResult> GetDashboardStats()
    {
        var dashboardData = await _unitOfWork.Dashboard.GetDashboardDataAsync();
        return Success(dashboardData);
    }
}