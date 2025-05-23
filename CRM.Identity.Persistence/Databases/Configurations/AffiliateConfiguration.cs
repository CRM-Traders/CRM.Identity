using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class AffiliateConfiguration : AuditableEntityTypeConfiguration<Affiliate>
{
    public override void Configure(EntityTypeBuilder<Affiliate> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Affiliate)); 

        builder.Property(a => a.Phone)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(a => a.Website)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(a => a.IsActive);
        builder.Property(up => up.UserId)
            .IsRequired();
        builder.HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}