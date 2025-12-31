using System.Text.Json.Serialization;

namespace IteraClient.Models;

public class AccessToken
{
    [JsonPropertyName("access_token")]
    public string Token { get; set; }
}
