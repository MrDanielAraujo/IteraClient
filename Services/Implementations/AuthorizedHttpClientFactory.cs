using System.Net.Http.Headers;
using IteraClient.Interfaces;

namespace IteraClient.Services.Implementations;

/// <summary>
/// Implementação da fábrica de HttpClients autorizados.
/// Segue os princípios SOLID:
/// - SRP: Apenas cria HttpClients configurados com autorização
/// - OCP: Pode ser estendido através da interface IAuthorizedHttpClientFactory
/// - DIP: Depende de abstrações (ITokenService, IHttpClientFactory)
/// </summary>
public class AuthorizedHttpClientFactory : IAuthorizedHttpClientFactory
{
    private readonly ITokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizedHttpClientFactory(
        ITokenService tokenService,
        IHttpClientFactory httpClientFactory)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <inheritdoc />
    public async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient("IteraApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
