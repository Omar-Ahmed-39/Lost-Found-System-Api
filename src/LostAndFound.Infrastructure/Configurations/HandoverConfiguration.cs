namespace LostAndFound.Infrastructure.Configurations;

public class HandoverConfiguration : IEntityTypeConfiguration<Handover>
{
    public void Configure(EntityTypeBuilder<Handover> builder)
    {
        builder.ToTable("Handovers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IdNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ImagePath)
            .HasMaxLength(250);

        builder.Property(x => x.HandoverDate)
            .IsRequired();

        builder.Property(n => n.Notes)
            .HasMaxLength(500);

        // Relationships

        // Handover -> Location (Many-to-One)
        builder.HasOne(h => h.Location)
            .WithMany(l => l.Handovers)
            .HasForeignKey(h => h.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Handover -> Receiver User (Many-to-One)
        builder.HasOne(h => h.ReceiverUser)
            .WithMany(u => u.ReceivedHandovers)
            .HasForeignKey(h => h.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Handover -> Given By User (Many-to-One)
        builder.HasOne(h => h.GivenByUser)
            .WithMany(u => u.HandledBy)
            .HasForeignKey(h => h.HandedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Handover -> Claim (One-to-One)
        builder.HasOne(h => h.Claim)
            .WithOne(c => c.Handover)
            .HasForeignKey<Handover>(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
