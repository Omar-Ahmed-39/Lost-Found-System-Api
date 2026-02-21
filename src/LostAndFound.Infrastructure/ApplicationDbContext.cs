namespace LostAndFound.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);    

    }
}
