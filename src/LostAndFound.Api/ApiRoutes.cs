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
    }
    public static class Matches
    {
        public const string GetAll = Base + "/matches";
        public const string GetPending = Base + "/matches/Pending";
        public const string GetById = Base + "/matches/{matchId}";
        public const string Verify = Base + "/matches/verify";
    }
    public static class Notifications
    {
        public const string GetUserNotifications = Base + "/notifications/me";
        public const string MarkAsRead = Base + "/notifications/{id}/read";
    }
    public static class Locations
    {
        public const string GetAll = Base + "/admin/locations";
        public const string GetById = Base + "/admin/locations/{id}";
        public const string Create = Base + "/admin/locations";
        public const string Update = Base + "/admin/locations/{id}";
        public const string Delete = Base + "/admin/locations/{id}";
    }
    public static class Categories
    {
        public const string GetAll = Base + "/admin/categories";
        public const string GetById = Base + "/admin/categories/{id}";
        public const string Create = Base + "/admin/categories";
        public const string Update = Base + "/admin/categories/{id}";
        public const string Delete = Base + "/admin/categories/{id}";
    }
}