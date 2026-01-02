namespace IteraClient.Interfaces;

/// <summary>
/// Interface responsável pela validação de tokens JWT.
/// Segue o princípio de Single Responsibility (SRP) - apenas valida tokens JWT.
/// </summary>
public interface IJwtValidator
{
    /// <summary>
    /// Verifica se um token JWT está expirado.
    /// </summary>
    /// <param name="token">Token JWT a ser validado</param>
    /// <returns>True se o token está expirado, False caso contrário</returns>
    bool IsTokenExpired(string? token);
}
