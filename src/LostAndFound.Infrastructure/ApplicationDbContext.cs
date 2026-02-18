namespace LostAndFound.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<ItemReport> ItemReports { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Handover> Handovers { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ItemAttachment> ItemAttachments { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);    

    }
}
