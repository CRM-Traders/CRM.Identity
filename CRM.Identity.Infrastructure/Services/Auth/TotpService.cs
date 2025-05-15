using OtpNet;

namespace CRM.Identity.Infrastructure.Services.Auth;

public class TotpService : ITotpService
{
    private const int KeySize = 32;
    private const int RecoveryCodeLength = 8;

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(KeySize);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string issuer, string accountName, string secret)
    {
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?" +
               $"secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
    }

    public bool ValidateCode(string secret, string code)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);

        return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
    }

    public List<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>();

        for (int i = 0; i < count; i++)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[RecoveryCodeLength];
            rng.GetBytes(bytes);
            codes.Add(Convert.ToBase64String(bytes).Replace("/", "").Replace("+", "").Substring(0, RecoveryCodeLength)
                .ToUpper());
        }

        return codes;
    }
}