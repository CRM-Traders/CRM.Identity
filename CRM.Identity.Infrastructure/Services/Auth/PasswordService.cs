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
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashOptions.iterations, HashOptions.hashAlgorithm,
            HashOptions.keySize);

        return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
    }

    public string GenerateStrongPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_-+=<>?";

        var password = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();

        password.Append(GetRandomChar(upperCase, rng));
        password.Append(GetRandomChar(lowerCase, rng));
        password.Append(GetRandomChar(digits, rng));
        password.Append(GetRandomChar(specialChars, rng));

        var allChars = upperCase + lowerCase + digits + specialChars;
        for (int i = 4; i < length; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        return ShuffleString(password.ToString(), rng);
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var data = new byte[4];
        rng.GetBytes(data);
        var value = BitConverter.ToUInt32(data, 0);
        return chars[(int)(value % (uint)chars.Length)];
    }

    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        var n = array.Length;
        while (n > 1)
        {
            var data = new byte[4];
            rng.GetBytes(data);
            var k = (int)(BitConverter.ToUInt32(data, 0) % (uint)n);
            n--;
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
    }
}