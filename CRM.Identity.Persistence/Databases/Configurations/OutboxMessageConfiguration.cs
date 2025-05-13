namespace CRM.Identity.Persistence.Databases.Configurations;

public class OutboxMessageConfiguration : BaseEntityTypeConfiguration<OutboxMessage>
{
    public override void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(OutboxMessage));

        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.Error)
            .HasMaxLength(2000);

        builder.Property(m => m.AggregateType)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(m => m.ClaimedBy)
            .HasMaxLength(100);

        builder.Property(m => m.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(m => new { m.ProcessedAt, m.IsClaimed });

        builder.HasIndex(m => m.Priority);

        builder.HasIndex(m => m.CreatedAt);

        builder.HasIndex(m => m.AggregateId);
    }
}