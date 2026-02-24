namespace LostAndFound.Infrastructure.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(x => x.Id);

        builder.Property(a => a.ApprovalStatus)
            .HasConversion<int>();

        builder.Property(a => a.Remarks)
            .HasMaxLength(300);

        builder.HasOne(x => x.Report)
            .WithMany(c => c.Claims)
            .HasForeignKey(x => x.ReportId);
    }
}