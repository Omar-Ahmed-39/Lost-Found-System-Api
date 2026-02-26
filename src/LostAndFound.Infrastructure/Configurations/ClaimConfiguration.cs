namespace LostAndFound.Infrastructure.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(x => x.Id);

        builder.Property(a => a.ApprovalStatus)
            .HasConversion<string>();

        builder.Property(a => a.Remarks)
            .HasMaxLength(300);

        // Claim -> ItemReport (Many-to-One)
        builder.HasOne(x => x.Report)
            .WithMany(c => c.Claims)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Restrict);

        // Claim -> User (Many-to-One)
        builder.HasOne(x => x.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}