using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class AffiliateSecretConfiguration : AuditableEntityTypeConfiguration<AffiliateSecret>
{
    public override void Configure(EntityTypeBuilder<AffiliateSecret> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(AffiliateSecret));

        builder.Property(s => s.AffiliateId)
            .IsRequired();

        builder.Property(s => s.SecretKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(s => s.SecretKey)
            .IsUnique();

        builder.Property(s => s.ExpirationDate)
            .IsRequired();

        builder.Property(s => s.IpRestriction)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.UsedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(s => s.Affiliate)
            .WithMany()
            .HasForeignKey(s => s.AffiliateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.AffiliateId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.ExpirationDate);
    }
}