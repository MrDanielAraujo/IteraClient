using System.Text;
using System.Text.Json;
using IteraClient.Configuration;
using IteraClient.Interfaces;
using IteraClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace IteraClient.Services.Implementations;

/// <summary>
/// Implementação do serviço de gerenciamento de tokens.
/// Segue os princípios SOLID:
/// - SRP: Apenas gerencia tokens de acesso
/// - OCP: Pode ser estendido através da interface ITokenService
/// - DIP: Depende de abstrações (IMemoryCache, IOptions, IJwtValidator)
/// </summary>
public class TokenService : ITokenService
{
    private const string CacheKey = "itera_access_token";
    
    private readonly IMemoryCache _cache;
    private readonly IJwtValidator _jwtValidator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IteraSettings _settings;

    public TokenService(
        IMemoryCache cache,
        IJwtValidator jwtValidator,
        IHttpClientFactory httpClientFactory,
        IOptions<IteraSettings> settings)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _jwtValidator = jwtValidator ?? throw new ArgumentNullException(nameof(jwtValidator));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Tenta obter do cache
        if (_cache.TryGetValue(CacheKey, out string? cachedToken))
        {
            if (!_jwtValidator.IsTokenExpired(cachedToken))
            {
                return cachedToken;
            }
        }

        // Solicita novo token
        var accessToken = await RequestTokenAsync(cancellationToken);
        
        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(55)); // Token geralmente expira em 60 min
        
        _cache.Set(CacheKey, accessToken.Token, cacheOptions);
        
        return accessToken.Token;
    }

    /// <summary>
    /// Realiza a requisição de token ao endpoint de autenticação.
    /// </summary>
    private async Task<AccessToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("IteraAuth");
        
        var authPayload = new
        {
            username = _settings.Auth.Username,
            password = _settings.Auth.Password
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(authPayload),
            Encoding.UTF8,
            "application/json");
        
        var response = await httpClient.PostAsync(_settings.Endpoints.Auth, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<AccessToken>(responseContent) ?? new AccessToken();
    }
}
