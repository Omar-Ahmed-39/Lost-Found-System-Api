using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LostAndFound.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet
    public DbSet<ItemReport> ItemReports { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Location> Locations { get; set; } = default!;
    public DbSet<Department> Departments { get; set; } = default!;
    public DbSet<University> Universities { get; set; } = default!;
    public DbSet<Claim> Claims { get; set; } = default!;
    public DbSet<Handover> Handovers { get; set; } = default!;
    public DbSet<Match> Matches { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;
    public DbSet<ItemAttachment> ItemAttachments { get; set; } = default!;
    public DbSet<Feedback> Feedbacks { get; set; } = default!;
    public DbSet<AuditLog> AuditLogs { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename ASP.NET Identity tables
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
