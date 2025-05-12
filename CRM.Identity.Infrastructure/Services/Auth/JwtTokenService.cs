namespace CRM.Identity.Infrastructure.Services.Auth;

public class JwtTokenService(JwtOptions _jwtOptions) : IJwtTokenService
{
    public string GenerateJwtToken(User user)
    {
        string base64PrivateKey = _jwtOptions.PrivateKey;

        byte[] privateKeyBytes = Convert.FromBase64String(base64PrivateKey);

        using RSA rsaPrivateKey = RSA.Create();

        rsaPrivateKey.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        var creds = new SigningCredentials(
                new RsaSecurityKey(rsaPrivateKey),
                SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var tokenClaims = new List<Claim>();

        tokenClaims.AddRange(new List<Claim>
        {
            new ("Uid", user.Id.ToString()),
            new ("Email", user.Email),
            new ("FullName", $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLower())
        });

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: tokenClaims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.AccessTokenValidityInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new(Convert.ToBase64String(randomNumber), DateTimeOffset.Now.AddMinutes(_jwtOptions.RefreshTokenValidityInMinutes));
    }

    public ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken)
    {
        validatedToken = null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            byte[] publicKeyBytes = Convert.FromBase64String(_jwtOptions.PublicKey);

            RSA rsaPublicKey = RSA.Create();
            rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
