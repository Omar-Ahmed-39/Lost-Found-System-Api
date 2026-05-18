namespace LostAndFound.Api;

public static class ApiRoutes
{
    public const string Root = "api";
    public const string Version = "v1";
    public const string Base = Root + "/" + Version;

    public static class Auth
    {
        public const string Login = Base + "/auth/login";
        public const string Register = Base + "/auth/register";
        public const string Refresh = Base + "/auth/refresh";
        public const string ChangePassword = Base + "/auth/change-password";
        public const string Logout = Base + "/auth/logout";
    }

    public static class Profile
    {
        public const string GetMyProfile = Base + "/profile/me";
        public const string UpdateMyProfile = Base + "/profile/me";
    }

    public static class Universities
    {
        public const string GetAll = Base + "/universities";
        public const string GetById = Base + "/universities/{id}";
        public const string Create = Base + "/admin/universities";
        public const string Update = Base + "/admin/universities/{id}";
        public const string Delete = Base + "/admin/universities/{id}";
    }

    public static class Departments
    {
        public const string GetAll = Base + "/departments";
        public const string GetById = Base + "/departments/{id}";
        public const string Create = Base + "/admin/departments";
        public const string Update = Base + "/admin/departments/{id}";
        public const string Delete = Base + "/admin/departments/{id}";
    }

    public static class Locations
    {
        public const string GetAll = Base + "/locations";
        public const string GetById = Base + "/locations/{id}";
        public const string Create = Base + "/admin/locations";
        public const string Update = Base + "/admin/locations/{id}";
        public const string Delete = Base + "/admin/locations/{id}";
    }

    public static class Categories
    {
        public const string GetAll = Base + "/categories";
        public const string GetById = Base + "/categories/{id}";
        public const string Create = Base + "/admin/categories";
        public const string Update = Base + "/admin/categories/{id}";
        public const string Delete = Base + "/admin/categories/{id}";
    }

    public static class Matches
    {
        // ── Admin routes ──────────────────────────────────────────────────────
        public const string GetAll     = Base + "/admin/matches";
        public const string GetPending = Base + "/admin/matches/pending";
        public const string GetById    = Base + "/admin/matches/{matchId}";
        public const string Verify     = Base + "/admin/matches/{matchId}/verify";

        // ── User / App routes ─────────────────────────────────────────────────
        /// <summary>GET  /api/v1/matches/{matchId}  — owner of the lost item only.</summary>
        public const string GetMyMatchById = Base + "/matches/{matchId}";
        /// <summary>POST /api/v1/matches/{matchId}/accept — owner of the lost item only.</summary>
        public const string Accept         = Base + "/matches/{matchId}/accept";
        /// <summary>POST /api/v1/matches/{matchId}/reject — owner of the lost item only.</summary>
        public const string Reject         = Base + "/matches/{matchId}/reject";
    }

    public static class Notifications
    {
        public const string GetUserNotifications = Base + "/notifications/me";
        public const string MarkAsRead = Base + "/notifications/{id}/read";
    }

    public static class Users
    {
        public const string GetAll = Base + "/admin/users";
        public const string GetById = Base + "/admin/users/{id}";
        public const string Create = Base + "/admin/users";
        public const string Update = Base + "/admin/users/{id}";
        public const string Delete = Base + "/admin/users/{id}";
        public const string ToggleBlock = Base + "/admin/users/{id}/block";
        public const string ChangeRole = Base + "/admin/users/{id}/role";
    }

    public static class Dashboard
    {
        public const string GetStats = Base + "/admin/dashboard/stats";
    }

    public static class Feedbacks
    {
        public const string GetAllAdmin = Base + "/admin/feedbacks";
        public const string Reply = Base + "/admin/feedbacks/{id}/reply";

        public const string Create = Base + "/feedbacks";
        public const string GetMyFeedbacks = Base + "/feedbacks/me";
    }
    public static class Reports
    {
        // User / App
        public const string GetAll = Base + "/reports";
        public const string GetById = Base + "/reports/{id}";
        public const string Create = Base + "/reports";
        public const string Update = Base + "/reports/{id}";
        public const string Cancel = Base + "/reports/{id}/cancel";
        public const string GetMyReports = Base + "/reports/me";

        // Admin
        public const string GetAllAdmin = Base + "/admin/reports";
        public const string GetByIdAdmin = Base + "/admin/reports/{id}";
        public const string Delete = Base + "/admin/reports/{id}";
        public const string ChangeStatus = Base + "/admin/reports/{id}/status";
        public const string ChangeReportType = Base + "/admin/reports/{id}/type";
    }
    public static class Claims
    {
        // User / App
        public const string Create = Base + "/claims";
        public const string GetMyClaims = Base + "/claims/me";
        public const string Cancel = Base + "/claims/{id}/cancel";

        // Admin
        public const string GetAll = Base + "/admin/claims";
        public const string GetById = Base + "/admin/claims/{id}";
        public const string Approve = Base + "/admin/claims/{id}/approve";
        public const string Reject = Base + "/admin/claims/{id}/reject";
    }
    public static class Handovers
    {
        public const string Create = Base + "/admin/handovers";
        public const string GetById = Base + "/admin/handovers/{id}";
        public const string GetByClaimId = Base + "/admin/handovers/claim/{claimId}";
    }
}