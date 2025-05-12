namespace CRM.Identity.Persistence.Databases.Configurations.Base;

public abstract class AuditableEntityTypeConfiguration<TEntity> : BaseEntityTypeConfiguration<TEntity> where TEntity : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(e => e.LastModifiedByIp)
            .HasMaxLength(45);

        builder.Property(e => e.DeletedBy)
            .HasMaxLength(100);

        builder.Property(e => e.DeletedByIp)
            .HasMaxLength(45);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}