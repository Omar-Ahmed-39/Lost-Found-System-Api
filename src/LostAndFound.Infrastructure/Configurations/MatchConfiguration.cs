using LostAndFound.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LostAndFound.Infrastructure.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MatchScore)
            .IsRequired()
            .HasColumnType("float");

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.MatchDate)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

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