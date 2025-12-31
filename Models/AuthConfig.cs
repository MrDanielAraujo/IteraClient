using System.Text.Json.Serialization;

namespace IteraClient.Models;

public class AuthConfig
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}
