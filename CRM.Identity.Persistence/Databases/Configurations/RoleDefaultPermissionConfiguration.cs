namespace CRM.Identity.Persistence.Databases.Configurations;

public class RoleDefaultPermissionConfiguration : BaseEntityTypeConfiguration<RoleDefaultPermission>
{
    public override void Configure(EntityTypeBuilder<RoleDefaultPermission> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(RoleDefaultPermission));

        builder.Property(rdp => rdp.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rdp => rdp.PermissionId)
            .IsRequired();

        builder.HasIndex(rdp => new { rdp.Role, rdp.PermissionId })
            .IsUnique();

        builder.HasOne(rdp => rdp.Permission)
            .WithMany()
            .HasForeignKey(rdp => rdp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}