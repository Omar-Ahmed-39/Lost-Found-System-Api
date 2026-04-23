using Microsoft.EntityFrameworkCore;
using LostAndFound.Core.Interfaces;
using LostAndFound.Core.Domain.Dashboard;
using LostAndFound.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.Data.SqlClient;
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
        var stats = new OverviewStats();

        // Use ADO.NET explicitly to run the Stored Procedure efficiently without configuring Keyless Entity
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "sp_GetOverviewStats";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@StartOfThisWeek", startOfThisWeek));
            command.Parameters.Add(new SqlParameter("@StartOfLastWeek", startOfLastWeek));

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    int activeReports = reader.GetInt32(reader.GetOrdinal("ActiveReports"));
                    int activeReportsThisWeek = reader.GetInt32(reader.GetOrdinal("ActiveReportsThisWeek"));
                    int pendingClaims = reader.GetInt32(reader.GetOrdinal("TotalPendingClaims"));
                    int pendingClaimsLastWeek = reader.GetInt32(reader.GetOrdinal("PendingClaimsLastWeek"));
                    double recoveryRate = reader.GetDouble(reader.GetOrdinal("RecoveryRate"));
                    int totalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsersCount"));
                    int newUsersThisWeek = reader.GetInt32(reader.GetOrdinal("NewUsersThisWeek"));

                    stats.ActiveReports = activeReports;
                    stats.ActiveReportsTrend = $"+{activeReportsThisWeek} this week";

                    stats.PendingClaims = pendingClaims;
                    stats.PendingClaimsTrend = pendingClaims > pendingClaimsLastWeek
                                            ? $"+{pendingClaims - pendingClaimsLastWeek} this week"
                                            : $"-{pendingClaimsLastWeek - pendingClaims} this week";

                    stats.RecoveryRate = recoveryRate;
                    stats.RecoveryRateTrend = "+0.0% this month";

                    stats.TotalUsers = totalUsers;
                    stats.TotalUsersTrend = $"+{newUsersThisWeek} new users";
                }
            }
            await _context.Database.CloseConnectionAsync();
        }

        return stats;
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
        // Only fetch raw data with raw DateTimes to avoid expensive string parsing allocations
        var recentReports = await _context.ItemReports
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new
            {
                Action = $"New {r.ReportType.ToString().ToLower()} report filed",
                Details = $"{r.User.Name} • {r.ItemName}",
                Status = r.ReportType.ToString(),
                Timestamp = r.CreatedAt
            })
            .ToListAsync();

        var recentClaims = await _context.Claims
            .Include(c => c.User)
            .Include(c => c.Report)
            .Where(c => c.ApprovalStatus == enApprovalStatus.Approved)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Take(5)
            .Select(c => new
            {
                Action = "Claim approved",
                Details = $"{c.User.Name} • {c.Report.ItemName}",
                Status = "Approved",
                Timestamp = c.UpdatedAt ?? c.CreatedAt
            })
            .ToListAsync();

        // Combine logic in memory: order by the REAL timestamp, take top 5, then parse time ago as string
        return recentReports.Concat(recentClaims)
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .Select(a => new RecentActivity
            {
                Action = a.Action,
                Details = a.Details,
                Status = a.Status,
                TimeAgo = CalculateTimeAgo(a.Timestamp)
            })
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
