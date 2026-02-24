namespace LostAndFound.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);

        // Relationships

        // Department -> University (Many-to-One)
        builder.HasOne(d => d.University)
            .WithMany(u => u.Departments)
            .HasForeignKey(d => d.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}