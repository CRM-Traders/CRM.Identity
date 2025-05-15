namespace CRM.Identity.Application.Common.Services.Auth;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string issuer, string accountName, string secret);
    bool ValidateCode(string secret, string code);
    List<string> GenerateRecoveryCodes(int count = 10);
}