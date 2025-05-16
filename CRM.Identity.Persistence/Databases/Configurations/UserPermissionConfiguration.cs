namespace CRM.Identity.Persistence.Databases.Configurations;

public class UserPermissionConfiguration : BaseEntityTypeConfiguration<UserPermission>
{
    public override void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        base.Configure(builder);
        builder.HasQueryFilter(up => !up.User.IsDeleted);

        builder.ToTable(nameof(UserPermission));

        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.PermissionId)
            .IsRequired();

        builder.Property(up => up.IsGranted)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(up => up.GrantedAt)
            .IsRequired();

        builder.Property(up => up.ExpiresAt)
            .IsRequired(false);

        builder.HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique()
            .HasFilter("\"IsGranted\" = true");

        builder.HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(up => up.Permission)
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}