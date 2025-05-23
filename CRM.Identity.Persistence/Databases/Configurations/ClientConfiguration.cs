using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class ClientConfiguration : AuditableEntityTypeConfiguration<Client>
{
    public override void Configure(EntityTypeBuilder<Client> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Client));

        builder.Property(c => c.AffiliateId)
            .IsRequired();

        builder.HasOne(c => c.Affiliate)
            .WithMany()
            .HasForeignKey(c => c.AffiliateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure base properties from ClientBase
        ConfigureClientBaseProperties(builder);

        builder.HasIndex(c => c.AffiliateId);
    }

    private static void ConfigureClientBaseProperties<T>(EntityTypeBuilder<T> builder) where T : ClientBase
    {
        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Telephone)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(c => c.SecondTelephone)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(c => c.Skype)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.Country)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Language)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.DateOfBirth)
            .IsRequired(false);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ClientStatus.Active);

        builder.Property(c => c.KycStatusId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.SalesStatus)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.IsProblematic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsBonusAbuser)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.BonusAbuserReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.HasInvestments)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.SecurityCode)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.ClientArea)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.IDPassportNumber)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.CountrySpecificIdentifier)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.MarketingType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.RegistrationNotes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(c => c.RegistrationDate)
            .IsRequired();

        builder.Property(c => c.RegistrationIP)
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(c => c.RegistrationSystem)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.RegistrationDevice)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.LastLogin)
            .IsRequired(false);

        builder.Property(c => c.LastCommunication)
            .IsRequired(false);

        builder.Property(c => c.LastUpdate)
            .IsRequired(false);

        builder.Property(c => c.Source)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.RefererType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.FTDTime)
            .IsRequired(false);

        builder.Property(c => c.LTDTime)
            .IsRequired(false);

        builder.Property(c => c.QualificationTime)
            .IsRequired(false);

        builder.Property(c => c.AllowTransactions)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.AnonymousCall)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(up => up.UserId)
            .IsRequired();
        builder.HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Indexes
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Country);
        builder.HasIndex(c => c.RegistrationDate);
        builder.HasIndex(c => c.IsProblematic);
        builder.HasIndex(c => c.IsBonusAbuser);
        builder.HasIndex(c => c.FTDTime);
        builder.HasIndex(c => c.Source);
    }
}