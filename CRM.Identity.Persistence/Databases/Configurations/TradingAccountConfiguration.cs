using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class TradingAccountConfiguration : AuditableEntityTypeConfiguration<TradingAccount>
{
    public override void Configure(EntityTypeBuilder<TradingAccount> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(TradingAccount));

        builder.Property(t => t.AccountLogin)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.AccountLogin)
            .IsUnique();

        builder.Property(t => t.Currency)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(t => t.Balance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.Leverage)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(t => t.Server)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.Equity)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.IsDemo)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.ClientId)
            .IsRequired();

        builder.HasOne(t => t.Client)
            .WithMany(c => c.TradingAccounts)
            .HasForeignKey(t => t.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.ClientId);
        builder.HasIndex(t => t.Currency);
        builder.HasIndex(t => t.IsDemo);
        builder.HasIndex(t => t.Balance);
        builder.HasIndex(t => new { t.ClientId, t.Currency });
    }
}