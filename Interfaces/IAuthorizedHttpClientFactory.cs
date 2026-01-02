namespace IteraClient.Interfaces;

/// <summary>
/// Interface responsável pela criação de HttpClients autorizados.
/// Segue o princípio de Single Responsibility (SRP) - apenas cria clientes HTTP configurados.
/// </summary>
public interface IAuthorizedHttpClientFactory
{
    /// <summary>
    /// Cria um HttpClient configurado com o token de autorização Bearer.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>HttpClient configurado com Bearer token</returns>
    Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken cancellationToken = default);
}
