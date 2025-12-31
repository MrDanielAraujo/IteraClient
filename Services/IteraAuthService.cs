using IteraClient.Models;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace IteraClient.Services;

public class IteraAuthService(AuthConfig config, EndPoints endPoints)
{
    private readonly AuthConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    public readonly EndPoints EndPoints = endPoints ?? throw new ArgumentNullException(nameof(endPoints));
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// Obtém um token de acesso válido, renovando automaticamente se necessário
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token de acesso Bearer válido</returns>
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache() // Adiciona o serviço de cache em memória
            .BuildServiceProvider();

        // Obtém a instância do IMemoryCache
        var cache = serviceProvider.GetService<IMemoryCache>();

        const string key = "accessToken";

        try
        {
            if (cache!.TryGetValue(key, out string? cacheValue))
                if (!IsJwtExpired(cacheValue)) return cacheValue;
            
            var accessToken = await RequestTokenAsync(cancellationToken);
            cache!.Set(key, accessToken.Token);
            return accessToken.Token;
        }
        catch (Exception ex) {

            throw new Exception(ex.Message);
        }

    }

    /// <summary>
    /// Cria um HttpClient configurado com o token de autorização
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>HttpClient configurado com Bearer token</returns>
    public async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    /// <summary>
    /// Realiza a requisição de token ao endpoint de autenticação
    /// </summary>
    private async Task<AccessToken> RequestTokenAsync(CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(_config), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(EndPoints.IteraAuth, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<AccessToken>(await response.Content.ReadAsStringAsync(cancellationToken)) ?? new AccessToken();
    }

    private static bool IsJwtExpired(string? token)
    {
        if (string.IsNullOrEmpty(token)) return true;
        
        return (new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo < DateTime.UtcNow);
    }
}
