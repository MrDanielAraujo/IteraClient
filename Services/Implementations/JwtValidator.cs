using System.IdentityModel.Tokens.Jwt;
using IteraClient.Interfaces;

namespace IteraClient.Services.Implementations;

/// <summary>
/// Implementação do validador de tokens JWT.
/// Segue o princípio de Single Responsibility (SRP) - apenas valida tokens JWT.
/// </summary>
public class JwtValidator : IJwtValidator
{
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtValidator()
    {
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <inheritdoc />
    public bool IsTokenExpired(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            // Se não conseguir ler o token, considera como expirado
            return true;
        }
    }
}
