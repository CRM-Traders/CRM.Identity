using System.Security.Cryptography;
using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.CreateAffiliateSecret;

public sealed record CreateAffiliateSecretCommand(
    DateTimeOffset? ExpirationDate = null, // Optional - defaults to 1 year
    string? IpRestriction = null) : IRequest<CreateAffiliateSecretResult>;

public sealed record CreateAffiliateSecretResult(
    Guid SecretId,
    string GeneratedSecretKey,
    Guid AffiliateId,
    DateTimeOffset ExpirationDate,
    string? IpRestriction);

public sealed class CreateAffiliateSecretCommandValidator : AbstractValidator<CreateAffiliateSecretCommand>
{
    public CreateAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpirationDate.HasValue);

        RuleFor(x => x.IpRestriction)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.IpRestriction))
            .WithMessage("IP restriction cannot exceed 500 characters.")
            .Must(BeValidIpRestriction)
            .When(x => !string.IsNullOrEmpty(x.IpRestriction))
            .WithMessage("IP restriction must contain valid IP addresses or CIDR blocks separated by commas");
    }

    private static bool BeValidIpRestriction(string? ipRestriction)
    {
        if (string.IsNullOrWhiteSpace(ipRestriction)) return true;

        var ips = ipRestriction.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var ip in ips)
        {
            var trimmedIp = ip.Trim();
            if (trimmedIp == "*") continue; // Allow wildcard

            if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedIp, @"^(\d{1,3}\.){3}\d{1,3}(\/\d{1,2})?$"))
            {
                return false;
            }
        }

        return true;
    }
}

public sealed class CreateAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<CreateAffiliateSecretCommand, CreateAffiliateSecretResult>
{
    public async ValueTask<Result<CreateAffiliateSecretResult>> Handle(CreateAffiliateSecretCommand request,
        CancellationToken cancellationToken)
    {
        // Get affiliate by current user ID
        var affiliateSpec = new AffiliateByUserIdSpecification(userContext.Id);
        var affiliate = await affiliateRepository.FirstOrDefaultAsync(affiliateSpec, cancellationToken);

        if (affiliate == null)
        {
            return Result.Failure<CreateAffiliateSecretResult>("No affiliate found for current user", "NotFound");
        }

        // Generate unique secret key
        var secretKey = GenerateUniqueSecretKey();
        var maxAttempts = 5;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            var secretKeySpecification = new AffiliateSecretBySecretKeySpecification(secretKey);
            var existingSecretByKey =
                await affiliateSecretRepository.FirstOrDefaultAsync(secretKeySpecification, cancellationToken);

            if (existingSecretByKey == null)
                break; // Key is unique

            secretKey = GenerateUniqueSecretKey();
            attempt++;
        }

        if (attempt >= maxAttempts)
        {
            return Result.Failure<CreateAffiliateSecretResult>("Failed to generate unique secret key",
                "InternalServerError");
        }

        // Set default expiration to 1 year if not provided
        var expirationDate = request.ExpirationDate ?? DateTimeOffset.UtcNow.AddYears(1);

        // Create affiliate secret
        var affiliateSecret = new AffiliateSecret(
            affiliate.Id,
            secretKey,
            expirationDate,
            request.IpRestriction);

        await affiliateSecretRepository.AddAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateAffiliateSecretResult(
            affiliateSecret.Id,
            secretKey,
            affiliate.Id,
            expirationDate,
            request.IpRestriction));
    }

    private static string GenerateUniqueSecretKey()
    { 
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        const int keyLength = 64;

        using var rng = RandomNumberGenerator.Create();
        var result = new char[keyLength];
        var buffer = new byte[4];

        for (int i = 0; i < keyLength; i++)
        {
            rng.GetBytes(buffer);
            var value = BitConverter.ToUInt32(buffer, 0);
            result[i] = chars[(int)(value % (uint)chars.Length)];
        }

        return new string(result);
    }
}