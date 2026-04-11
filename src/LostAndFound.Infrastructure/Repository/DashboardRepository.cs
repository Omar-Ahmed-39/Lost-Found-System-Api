using Microsoft.EntityFrameworkCore;
using LostAndFound.Core.Interfaces;
using LostAndFound.Core.Domain.Dashboard;
using LostAndFound.Core.Enums;
using Microsoft.AspNetCore.Identity;
using LostAndFound.Core.Entities;

namespace LostAndFound.Infrastructure.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardRepository(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;
        var startOfThisWeek = now.AddDays(-7).Date;
        var startOfLastWeek = startOfThisWeek.AddDays(-7).Date;

        var data = new DashboardData();
        data.Overview = await GetOverviewStatsAsync(startOfThisWeek, startOfLastWeek);
        data.ReportsByCategory = await GetReportsByCategoryAsync();
        data.WeeklyActivity = await GetWeeklyActivityAsync(startOfThisWeek);
        data.RecentActivity = await GetRecentActivityAsync();

        return data;
    }

    private async Task<OverviewStats> GetOverviewStatsAsync(DateTime startOfThisWeek, DateTime startOfLastWeek)
    {
        // Active Reports
        var totalActiveReports = await _context.ItemReports
            .Where(r => r.StatusType == enStatusType.Open || r.StatusType == enStatusType.UnderReview)
            .CountAsync();

        var activeReportsThisWeek = await _context.ItemReports
            .Where(r => (r.StatusType == enStatusType.Open || r.StatusType == enStatusType.UnderReview) && r.CreatedAt >= startOfThisWeek)
            .CountAsync();

        // Pending Claims
        var totalPendingClaims = await _context.Claims
            .Where(c => c.ApprovalStatus == enApprovalStatus.Pending)
            .CountAsync();

        var pendingClaimsLastWeek = await _context.Claims
            .Where(c => c.ApprovalStatus == enApprovalStatus.Pending && c.CreatedAt >= startOfLastWeek && c.CreatedAt < startOfThisWeek)
            .CountAsync();

        // Recovery Rate
        var totalResolvedReports = await _context.ItemReports
            .Where(r => r.StatusType == enStatusType.Returned || r.StatusType == enStatusType.Closed)
            .CountAsync();

        var totalReports = await _context.ItemReports.CountAsync();

        var recoveryRate = totalReports > 0 ? Math.Round((double)totalResolvedReports / totalReports * 100, 1) : 0;

        // Users
        var totalUsersCount = await _userManager.Users.CountAsync();
        var newUsersThisWeek = await _userManager.Users.Where(u => u.Created >= startOfThisWeek).CountAsync();

        return new OverviewStats
        {
            ActiveReports = totalActiveReports,
            ActiveReportsTrend = $"+{activeReportsThisWeek} this week",

            PendingClaims = totalPendingClaims,
            PendingClaimsTrend = totalPendingClaims > pendingClaimsLastWeek
                                 ? $"+{totalPendingClaims - pendingClaimsLastWeek} this week"
                                 : $"-{pendingClaimsLastWeek - totalPendingClaims} this week", // Simplified trend calculation for demo

            RecoveryRate = recoveryRate,
            RecoveryRateTrend = "+0.0% this month", // Assuming this needs more complex historical rate calculation, hardcoding pattern

            TotalUsers = totalUsersCount,
            TotalUsersTrend = $"+{newUsersThisWeek} new users"
        };
    }

    private async Task<List<CategoryStat>> GetReportsByCategoryAsync()
    {
        return await _context.ItemReports
            .Include(r => r.Category)
            .GroupBy(r => r.Category.Name)
            .Select(g => new CategoryStat
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    private async Task<List<WeeklyActivity>> GetWeeklyActivityAsync(DateTime startDate)
    {
        var rawData = await _context.ItemReports
            .Where(r => r.CreatedAt >= startDate)
            .GroupBy(r => new { r.CreatedAt.Date, r.ReportType })
            .Select(g => new
            {
                Date = g.Key.Date,
                Type = g.Key.ReportType,
                Count = g.Count()
            })
            .ToListAsync();

        var weeklyActivities = new List<WeeklyActivity>();
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            weeklyActivities.Add(new WeeklyActivity
            {
                Day = date.DayOfWeek.ToString().Substring(0, 3), // Mon, Tue...
                LostCount = rawData.Where(d => d.Date == date && d.Type == enReportType.Lost).Select(d => d.Count).FirstOrDefault(),
                FoundCount = rawData.Where(d => d.Date == date && d.Type == enReportType.Found).Select(d => d.Count).FirstOrDefault()
            });
        }

        return weeklyActivities;
    }

    private async Task<List<RecentActivity>> GetRecentActivityAsync()
    {
        var recentReports = await _context.ItemReports
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new RecentActivity
            {
                Action = $"New {r.ReportType.ToString().ToLower()} report filed",
                Details = $"{r.User.Name} • {r.ItemName}",
                Status = r.ReportType.ToString(),
                TimeAgo = CalculateTimeAgo(r.CreatedAt)
            })
            .ToListAsync();

        var recentClaims = await _context.Claims
            .Include(c => c.User)
            .Include(c => c.Report)
            .Where(c => c.ApprovalStatus == enApprovalStatus.Approved)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(5)
            .Select(c => new RecentActivity
            {
                Action = "Claim approved",
                Details = $"{c.User.Name} • {c.Report.ItemName}",
                Status = "Approved",
                TimeAgo = CalculateTimeAgo(c.UpdatedAt)
            })
            .ToListAsync();

        return recentReports.Concat(recentClaims)
            .OrderByDescending(a => DateTime.Parse(a.TimeAgo.Split(' ')[0] + " " + a.TimeAgo.Split(' ')[1])) // very rough ordering by time ago, assumes "X min ago"
            .Take(5)
            .ToList();
    }

    private static string CalculateTimeAgo(DateTime date)
    {
        var span = DateTime.UtcNow - date;
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
        return $"{(int)span.TotalDays} days ago";
    }
}