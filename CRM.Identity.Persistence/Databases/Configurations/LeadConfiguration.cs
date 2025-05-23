using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Persistence.Databases.Configurations;

public class LeadConfiguration : AuditableEntityTypeConfiguration<Lead>
{
    public override void Configure(EntityTypeBuilder<Lead> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(Lead));

        // Configure base properties from ClientBase
        ConfigureClientBaseProperties(builder);
    }

    private static void ConfigureClientBaseProperties(EntityTypeBuilder<Lead> builder)
    {
        builder.Property(l => l.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(l => l.Email)
            .IsUnique();

        builder.Property(l => l.Telephone)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(l => l.SecondTelephone)
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(l => l.Skype)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.Country)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.Language)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.DateOfBirth)
            .IsRequired(false);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ClientStatus.Active);

        builder.Property(l => l.KycStatusId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.SalesStatus)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.IsProblematic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.IsBonusAbuser)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.BonusAbuserReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(l => l.HasInvestments)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.SecurityCode)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.ClientArea)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.IDPassportNumber)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.CountrySpecificIdentifier)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.MarketingType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.RegistrationNotes)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(l => l.RegistrationDate)
            .IsRequired();

        builder.Property(l => l.RegistrationIP)
            .HasMaxLength(45)
            .IsRequired(false);

        builder.Property(l => l.RegistrationSystem)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.RegistrationDevice)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(l => l.LastLogin)
            .IsRequired(false);

        builder.Property(l => l.LastCommunication)
            .IsRequired(false);

        builder.Property(l => l.LastUpdate)
            .IsRequired(false);

        builder.Property(l => l.Source)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.RefererType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.FTDTime)
            .IsRequired(false);

        builder.Property(l => l.LTDTime)
            .IsRequired(false);

        builder.Property(l => l.QualificationTime)
            .IsRequired(false);

        builder.Property(l => l.AllowTransactions)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.AnonymousCall)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(up => up.UserId)
            .IsRequired();
        builder.HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // Indexes
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.Country);
        builder.HasIndex(l => l.RegistrationDate);
        builder.HasIndex(l => l.IsProblematic);
        builder.HasIndex(l => l.IsBonusAbuser);
        builder.HasIndex(l => l.Source);
    }
}