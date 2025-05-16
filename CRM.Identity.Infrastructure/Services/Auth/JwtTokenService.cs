using System.Collections;

namespace CRM.Identity.Infrastructure.Services.Auth;

public class JwtTokenService(JwtOptions jwtOptions, IServiceProvider serviceProvider) : IJwtTokenService
{
    public string GenerateJwtToken(User user)
    {
        string base64PrivateKey = jwtOptions.PrivateKey;

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

        var expirationTime = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenValidityInMinutes);

        tokenClaims.AddRange(new List<Claim>
        {
            new("Uid", user.Id.ToString()),
            new("Email", user.Email),
            new("FullName", $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLower()),
            new("exp", new DateTimeOffset(expirationTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("expire_at", expirationTime.ToString("O"), ClaimValueTypes.DateTime)
        });

        using (var scope = serviceProvider.CreateScope())
        {
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            var userPermissions = permissionService.GetUserPermissionsAsync(user.Id).GetAwaiter().GetResult();
             
            var permissionBinary = permissionService.GeneratePermissionBinary(userPermissions);
            tokenClaims.Add(new Claim("permission", permissionBinary));
             
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: tokenClaims,
            expires: expirationTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    } 

    public RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new(Convert.ToBase64String(randomNumber),
            DateTimeOffset.Now.AddMinutes(jwtOptions.RefreshTokenValidityInMinutes));
    }

    public ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken)
    {
        validatedToken = null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            byte[] publicKeyBytes = Convert.FromBase64String(jwtOptions.PublicKey);

            RSA rsaPublicKey = RSA.Create();
            rsaPublicKey.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
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