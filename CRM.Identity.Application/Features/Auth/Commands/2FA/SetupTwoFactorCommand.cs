using Microsoft.Extensions.Logging;

namespace CRM.Identity.Application.Features.Auth.Commands._2FA;

public sealed record SetupTwoFactorCommand() : IRequest<SetupTwoFactorResponse>;

public sealed class SetupTwoFactorCommandHandler(
    IRepository<User> userRepository,
    ITotpService totpService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<SetupTwoFactorCommandHandler> logger) : IRequestHandler<SetupTwoFactorCommand, SetupTwoFactorResponse>
{
    private const string AppName = "CRM";

    public async ValueTask<Result<SetupTwoFactorResponse>> Handle(
        SetupTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure<SetupTwoFactorResponse>("User not found", "NotFound");
        }

        if (user.IsTwoFactorEnabled && user.IsTwoFactorVerified)
        {
            return Result.Failure<SetupTwoFactorResponse>("2FA already exist", "Conflict");
        }

        var secret = totpService.GenerateSecret();
        var qrCodeUri = totpService.GenerateQrCodeUri(AppName, user.Email, secret);

        user.EnableTwoFactorAuthentication(secret);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("2FA setup initiated for user {UserId}", user.Id);

        return Result.Success(new SetupTwoFactorResponse
        {
            Secret = secret,
            QrCodeUri = qrCodeUri,
            ManualEntryKey = FormatSecretForManualEntry(secret),
            AppName = AppName
        });
    }

    private string FormatSecretForManualEntry(string secret)
    {
        var cleanSecret = secret.TrimEnd('=');

        var chunks = new List<string>();
        for (int i = 0; i < cleanSecret.Length; i += 4)
        {
            chunks.Add(cleanSecret.Substring(i, Math.Min(4, cleanSecret.Length - i)));
        }

        return string.Join(" ", chunks);
    }
}

public sealed record SetupTwoFactorResponse
{
    public string Secret { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
}