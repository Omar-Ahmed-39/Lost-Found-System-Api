namespace LostAndFound.Infrastructure.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MatchScore)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.MatchDate)
            .IsRequired();

        // Prevent duplicate matches for the same Lost+Found pair.
        // This is the DB-level safety net; the application-level dedup guard
        // in MatchingService is the first line of defence.
        builder.HasIndex(m => new { m.LostId, m.FoundId }).IsUnique();

        // Relationships

        // Match -> LostItem (many-to-one)
        builder.HasOne(m => m.LostItem)
            .WithMany(li => li.LostMatches)
            .HasForeignKey(m => m.LostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> FoundItem (many-to-one)
        builder.HasOne(m => m.FoundItem)
            .WithMany(fi => fi.FoundMatches)
            .HasForeignKey(m => m.FoundId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> MatchedByUser (many-to-one)
        builder.HasOne(m => m.MatchedByUser)
            .WithMany(u => u.Matches)
            .HasForeignKey(m => m.MatchedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}