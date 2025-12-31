using IteraClient.Models;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace IteraClient.Services;

public class IteraAuthService
{
    private readonly AuthConfig _config;
    private readonly EndPoints _endPoints;
    private readonly HttpClient _httpClient;

    public IteraAuthService(AuthConfig config, EndPoints endPoints)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _endPoints = endPoints ?? throw new ArgumentNullException(nameof(endPoints));
        _httpClient = new HttpClient();
    }

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

        string key = "accessToken";

        try
        {
            if (!cache.TryGetValue(key, out string? cacheValue))
            {
                var accessToken = await RequestTokenAsync(cancellationToken);
                cache.Set(key, accessToken.Token);
                cacheValue = accessToken.Token;
            }
            else
            {
                if (!IsJwtExpired(cacheValue))
                {
                    var accessToken = await RequestTokenAsync(cancellationToken);
                    cache.Set(key, accessToken.Token);
                    cacheValue = accessToken.Token;
                }
            }

            return cacheValue;
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
        string jsonString = JsonSerializer.Serialize(_config);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_endPoints.IteraAuth, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        var accessToken = JsonSerializer.Deserialize<AccessToken>(result);
        return accessToken ?? throw new ArgumentNullException("Access Token Invalido");
    }


    public bool IsJwtExpired(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return true; // Ou false, dependendo da sua lógica para tokens nulos/vazios
        }

        var handler = new JwtSecurityTokenHandler();
        try
        {
            // Lê o token sem validar a assinatura
            var jwtToken = handler.ReadJwtToken(token);

            // A propriedade ValidTo já considera o fuso horário UTC e o ClockSkew padrão de 5 minutos.
            // Comparar diretamente com DateTime.UtcNow é a maneira correta.
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return true; // Token expirado
            }

            return false; // Token ainda válido
        }
        catch (ArgumentException)
        {
            // Tratar caso o token não seja um JWT válido (ex: formato incorreto)
            return true;
        }
        catch (Exception)
        {
            // Tratar outros possíveis erros de leitura/parsing
            return true;
        }
    }
}
