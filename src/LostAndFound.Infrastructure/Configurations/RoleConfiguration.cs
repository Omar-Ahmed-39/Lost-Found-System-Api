using Microsoft.AspNetCore.Identity;

namespace LostAndFound.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        // builder.Property(r => r.Name) // Identity handles Name uniqueness and constraints
        //    .IsRequired()
        //    .HasMaxLength(50);

        // Relationship configuration for many-to-many with User through IdentityUserRole<int>
        builder
            .HasMany(r => r.Users)
            .WithMany(u => u.Roles)
            .UsingEntity<IdentityUserRole<int>>(
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired(),
                j => j
                    .HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired()
            );
    }
}