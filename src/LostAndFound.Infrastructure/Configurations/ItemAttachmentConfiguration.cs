using LostAndFound.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LostAndFound.Infrastructure.Configurations;

public class ItemAttachmentConfiguration : IEntityTypeConfiguration<ItemAttachment>
{
    public void Configure(EntityTypeBuilder<ItemAttachment> builder)
    {
        builder.ToTable("ItemAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationship 

        // ItemAttachment -> ItemReport (Many-to-One)
        builder.HasOne(x => x.ItemReport)
            .WithMany(r => r.Attachments)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}