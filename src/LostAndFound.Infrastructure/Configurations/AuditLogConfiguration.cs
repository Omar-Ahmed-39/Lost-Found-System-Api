namespace LostAndFound.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(300);
        builder.Property(al => al.Target)
            .IsRequired()
            .HasMaxLength(300);
        builder.Property(al => al.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // Max length for IPv6 addresses

        // Relationships
        builder.HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserId);
    }
}