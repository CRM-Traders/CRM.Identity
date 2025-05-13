namespace CRM.Identity.Persistence.Databases.Configurations;

public class PermissionConfiguration : BaseEntityTypeConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Permission));

        builder.Property(p => p.Order)
            .IsRequired();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Section)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(200);

        builder.Property(p => p.ActionType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.AllowedRoles)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => new { p.Section, p.Title, p.ActionType })
            .IsUnique();

        builder.HasIndex(p => p.Order)
            .IsUnique();
    }
}