namespace CRM.Identity.Application.Common.Services.Auth;

public interface IPasswordService
{
    string HashPasword(string password, out byte[] salt);
    bool VerifyPassword(string password, string hash, byte[] salt);
    string GenerateStrongPassword(int length = 12);
}