namespace LostAndFound.Infrastructure.Configurations;

public class ItemReportConfiguration : IEntityTypeConfiguration<ItemReport>
{
    public void Configure(EntityTypeBuilder<ItemReport> builder)
    {
        builder.ToTable("ItemReports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Color)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.AdminNotes)
            .HasMaxLength(500);

        builder.Property(x => x.ReportType)
            .HasConversion<string>();

        builder.Property(x => x.ConditionType)
            .HasConversion<string>();

        builder.Property(x => x.StatusType)
            .HasConversion<string>();

        // Relationships

        // ItemReport - User (Many-to-One)

        builder.HasOne(x => x.User)
            .WithMany(u => u.Reports)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ItemReport - Location (Many-to-One)
        builder.HasOne(x => x.Location)
            .WithMany(l => l.Reports)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ItemReport - Category (Many-to-One)
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Reports)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}