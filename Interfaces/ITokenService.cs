namespace IteraClient.Interfaces;

/// <summary>
/// Interface responsável pelo gerenciamento de tokens de acesso.
/// Segue o princípio de Single Responsibility (SRP) - apenas gerencia tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Obtém um token de acesso válido, renovando automaticamente se necessário.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token de acesso Bearer válido</returns>
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
