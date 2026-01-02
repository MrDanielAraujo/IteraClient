using IteraClient.Interfaces;
using IteraClient.Services.Implementations;

namespace IteraClient.Configuration;

/// <summary>
/// Extensões para configuração de serviços no container de DI.
/// Centraliza o registro de todos os serviços da aplicação.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços do Itera ao container de injeção de dependência.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddIteraServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração tipada usando Options Pattern
        services.Configure<IteraSettings>(configuration.GetSection(IteraSettings.SectionName));
        
        // Registra HttpClient usando IHttpClientFactory (melhor prática para gerenciamento de conexões)
        services.AddHttpClient("IteraAuth");
        services.AddHttpClient("IteraApi");
        
        // Registra serviços de cache
        services.AddMemoryCache();
        
        // Registra serviços da aplicação
        // Singleton: IJwtValidator - não possui estado mutável
        services.AddSingleton<IJwtValidator, JwtValidator>();
        
        // Scoped: Serviços que podem ter estado por requisição
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthorizedHttpClientFactory, AuthorizedHttpClientFactory>();
        services.AddScoped<IIteraApiClient, IteraApiClient>();
        
        return services;
    }
}
