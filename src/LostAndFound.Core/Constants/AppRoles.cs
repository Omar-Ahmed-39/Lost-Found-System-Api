namespace LostAndFound.Core.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    public const string User = "User";
    
    public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
}
