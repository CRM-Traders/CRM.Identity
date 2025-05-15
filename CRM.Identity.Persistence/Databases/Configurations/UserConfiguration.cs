using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class UserConfiguration : AuditableEntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(User));

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.PasswordSalt)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
 
        builder.Property(u => u.IsTwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.IsTwoFactorVerified)
            .IsRequired()
            .HasDefaultValue(false);
 
        var recoveryCodesConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>());

        var recoveryCodesComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(u => u.RecoveryCodes)
            .HasConversion(recoveryCodesConverter)
            .Metadata.SetValueComparer(recoveryCodesComparer);

        builder.Property(u => u.RecoveryCodes)
            .HasColumnType("jsonb")  
            .IsRequired(false)
            .HasDefaultValueSql("'[]'::jsonb");
 
        builder.HasIndex(u => u.IsTwoFactorEnabled)
            .HasFilter("\"IsTwoFactorEnabled\" = true");
    }
}