namespace LostAndFound.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        builder.Property(u => u.Created)
             .IsRequired()
       .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        // Relationships

        // User - ItemReport (One-to-many)
        builder.HasMany(u => u.Reports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Claim (One-to-many)
        builder.HasMany(u => u.Claims)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Notification (One-to-many)
        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - MatchByUser (One-to-many)
        builder.HasMany(u => u.Matches)
            .WithOne(m => m.MatchedByUser)
            .HasForeignKey(m => m.MatchedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Handover (handled by) (One-to-many)
        builder.HasMany(u => u.HandledBy)
            .WithOne(h => h.GivenByUser)
            .HasForeignKey(h => h.HandedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User - Handover (received) (One-to-many)
        builder.HasMany(u => u.ReceivedHandovers)
            .WithOne(h => h.ReciverUser)
            .HasForeignKey(h => h.ReciverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}