namespace CRM.Identity.Infrastructure.Services.Auth;

public class PasswordService : IPasswordService
{
    public string HashPasword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(HashOptions.keySize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            HashOptions.iterations,
            HashOptions.hashAlgorithm,
            HashOptions.keySize);

        return Convert.ToHexString(hash);
    }

    public bool VerifyPassword(string password, string hash, byte[] salt)
    {
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashOptions.iterations, HashOptions.hashAlgorithm, HashOptions.keySize);

        return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
    }
}
