namespace LostAndFound.Infrastructure.Configurations;

public class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.ToTable("Universities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Relationships

        // University -> Departments (One-to-Many)
        builder.HasMany(u => u.Departments)
            .WithOne(d => d.University)
            .HasForeignKey(d => d.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}