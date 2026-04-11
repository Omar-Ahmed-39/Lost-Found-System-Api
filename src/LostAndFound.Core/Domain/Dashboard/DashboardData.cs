namespace LostAndFound.Core.Domain.Dashboard;

public class DashboardData
{
    public OverviewStats Overview { get; set; } = new();
    public List<CategoryStat> ReportsByCategory { get; set; } = new();
    public List<WeeklyActivity> WeeklyActivity { get; set; } = new();
    public List<RecentActivity> RecentActivity { get; set; } = new();
}

public class OverviewStats
{
    public int ActiveReports { get; set; }
    public string ActiveReportsTrend { get; set; } = string.Empty;
    public int PendingClaims { get; set; }
    public string PendingClaimsTrend { get; set; } = string.Empty;
    public double RecoveryRate { get; set; }
    public string RecoveryRateTrend { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public string TotalUsersTrend { get; set; } = string.Empty;
}

public class CategoryStat
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WeeklyActivity
{
    public string Day { get; set; } = string.Empty;
    public int LostCount { get; set; }
    public int FoundCount { get; set; }
}

public class RecentActivity
{
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}