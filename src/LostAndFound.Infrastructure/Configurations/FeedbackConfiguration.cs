namespace LostAndFound.Infrastructure.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
       public void Configure(EntityTypeBuilder<Feedback> builder)
       {
              builder.ToTable("Feedbacks");

              builder.HasKey(f => f.Id);

              builder.Property(f => f.Subject)
                     .HasMaxLength(150)
                     .IsRequired();

              builder.Property(f => f.Message)
                     .HasMaxLength(1000)
                     .IsRequired();

              builder.HasOne(f => f.User)
                     .WithMany(u => u.Feedbacks)
                     .HasForeignKey(f => f.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}
