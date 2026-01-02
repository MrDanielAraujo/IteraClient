using System.Text.Json.Serialization;

namespace IteraClient.Models;

/// <summary>
/// Modelo que representa o token de acesso retornado pela API de autenticação.
/// </summary>
public class AccessToken
{
    /// <summary>
    /// Token de acesso JWT.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string Token { get; set; } = string.Empty;
}
