namespace LostAndFound.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.LocationType)
            .HasConversion<string>()
            .IsRequired();

        // Relationships

        // Location - Department (Many-to-One)
        builder.HasOne(l => l.Department)
            .WithMany(d => d.Locations)
            .HasForeignKey(l => l.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}